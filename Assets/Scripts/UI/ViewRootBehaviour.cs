using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Client.Attribute;
using Client.Data;
using Client.Extention;
using Client.UI.Define;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace Client.UI
{
    public class ViewRootBehaviour : MonoBehaviour
    {
        [SerializeField] internal Transform _topLayer;
        [SerializeField] internal Transform _popupLayer;
        [SerializeField] internal Transform _hudLayer;
        [SerializeField] internal Transform _objectLayer;


        [SerializeField] private UniversalRendererData _forwardRendererData;
        private readonly Dictionary<string, GameObject> _cahcedPrefabDictionary = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly List<IUiView> _focusingUis = new();
        private int _lastScreenHeight;
        private int _lastScreenWidth;

        public static string RootPath => "UI/Prefabs";

        //public UIGlobalPool GlobalUIPool { get; private set; }

        public static ViewRootBehaviour Instance { get; private set; }

        private IEnumerable<Transform> AllLayer
        {
            get
            {
                yield return _topLayer;
                yield return _popupLayer;
                yield return _hudLayer;
                yield return _objectLayer;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Initialize();
            }
            else
            {
                if (Instance != this) Destroy(gameObject);
            }
        }

        public void OnDestroy()
        {
            _focusingUis.Clear();
            _cahcedPrefabDictionary.Clear();

            if (_disposables != null) _disposables.Dispose();
        }

        public static int SortByOrder((MonoBehaviour View, int Order) x, (MonoBehaviour View, int Order) y)
        {
            return x.Order.CompareTo(y.Order);
        }

        private void Initialize()
        {
            Instance = this;
            gameObject.DontDestroyOnLoad2();

            // EventBetter.ListenManual((OnSceneEnterComplete msg) => { SceneService_OnComplete(); }).AddTo(_disposables);
            // EventBetter.ListenManual((OnSceneExit msg) => { SceneService_OnSceneExit(); }).AddTo(GlobalAPI.AppLevelDisposables);
            // EventBetter.ListenManual((OnSceneEntered msg) => { SceneService_OnSceneEntered(); }).AddTo(GlobalAPI.AppLevelDisposables);

            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
                {
                    _lastScreenWidth = Screen.width;
                    _lastScreenHeight = Screen.height;
                }
            }).AddTo(this);

            Debug.Log($"{GetType()} Initialize");
        }

        private void SceneService_OnComplete()
        {
            var eventSystem = GetComponent<EventSystem>();
            if (eventSystem != null) eventSystem.enabled = true;
        }

        private void SetLayer(GameObject go, Type type)
        {
            if (type.GetCustomAttribute<UiViewAttribute>() is { } attribute)
                go.transform.SetParent(
                    attribute.Layer switch
                    {
                        EUILayer.Top => _topLayer,
                        EUILayer.Popup => _popupLayer,
                        EUILayer.Hud => _hudLayer,
                        EUILayer.Object => _objectLayer,
                        _ => _hudLayer
                    },
                    false
                );
            else
                go.transform.SetParent(_hudLayer, false);
        }

        private int? GetOrder(Type type)
        {
            if (type.GetCustomAttribute<UiViewAttribute>() is { } attribute && attribute.Order != int.MaxValue)
                return attribute.Order;

            return default;
        }

        private string GetPrefabName(Type type, string path = default)
        {
            if (path.IsNullOrEmpty() == false) return $"{RootPath}/{path}";

            var attributes = type.GetCustomAttribute<UiViewAttribute>();
            if (attributes != default)
            {
                var prefabPath = attributes.PrefabPath;
                return $"{RootPath}/{prefabPath}";
            }

            return type.Name;
        }

        public bool IsRequireFocusing(Type type, out EUIFocusState focusState)
        {
            var attribute = type.GetCustomAttribute<UiViewAttribute>();

            var isFocusing = attribute != default && attribute.FocusState != EUIFocusState.None;
            if (isFocusing)
                focusState = attribute.FocusState;
            else
                focusState = EUIFocusState.None;

            return isFocusing;
        }

        public MonoBehaviour Create(Type viewType)
        {
            if (CreateViewObject(viewType) is var go && go != default)
            {
                var component = Initialize(viewType, go);
                return component;
            }

            return default;
        }

        private GameObject CreateViewObject(Type type, string path = default)
        {
            var prefabPath = GetPrefabName(type, path);

            if (_cahcedPrefabDictionary.TryGetValue(prefabPath, out var cachedPrefab))
            {
                var go = Instantiate(cachedPrefab);
                return go;
            }

            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != default)
            {
                _cahcedPrefabDictionary[prefabPath] = prefab;
                var go = Instantiate(prefab);
                return go;
            }

            Debug.LogError($"ViewRootBehaviour.Create NotExist : {type.Name}");
            return default;
        }

        public bool Destroy(MonoBehaviour behaviour, bool removeCache)
        {
            if (behaviour != default)
                foreach (var transform in AllLayer)
                    if (transform.GetComponentInChildren(behaviour.GetType(), true) is var component &&
                        component != default)
                    {
                        Destroy(component.gameObject);
                        var prefabPath = GetPrefabName(behaviour.GetType());
                        if (removeCache && _cahcedPrefabDictionary.TryGetValue(prefabPath, out var go))
                            _cahcedPrefabDictionary.Remove(prefabPath);
                        // Debug.Log($"<<<< CachedPrefab Unloaded : {behaviour.GetType()}");
                        return true;
                    }

            return false;
        }

        private MonoBehaviour Initialize(Type type, GameObject go)
        {
            if (go.GetComponent(type) is var ui && ui == default)
            {
                if (go.AddComponent(type) is var component && component != default)
                {
                    Prepare(go, component.GetType());

                    var monoBehaviour = component as MonoBehaviour;
                    if (component as IUiView is var view)
                    {
                        Close(monoBehaviour);
                        view.Loaded = true;
                    }

                    return monoBehaviour;
                }

                return default;
            }

            return default;
        }

        public void Open(MonoBehaviour behaviour)
        {
            if (behaviour != default)
            {
                behaviour.transform.SetAsLastSibling();
                behaviour.gameObject.SetActive(true);

                if (behaviour as IUiView is var view && view != default) view.Opened = true;

                SortUiElementInLayer(behaviour.transform.parent);

                if (IsRequireFocusing(behaviour.GetType(), out var focusState))
                {
                    if (focusState == EUIFocusState.Fullscreen)
                    {
                        //GlobalAPI.AppEnv.UIFocusState = focusState;
                    }
                    else if (focusState == EUIFocusState.Modal)
                    {
                        //GlobalAPI.AppEnv.UIFocusState = EUIFocusState.Modal;
                    }

                    // 팝업과 같은 Focusing Ui(FocusUiAttribute)는 추가
                    if (_focusingUis.FirstOrDefault(t => t == view) == default) _focusingUis.Add(view);
                }
            }
        }

        public void Close(MonoBehaviour behaviour)
        {
            if (behaviour != default)
            {
                behaviour.gameObject.SetActive(false);

                if (behaviour as IUiView is var view && view != default) view.Opened = false;

                if (IsRequireFocusing(behaviour.GetType(), out var focusState))
                    // 팝업과 같은 Focusing Ui(FocusUiAttribute)는 제거
                    if (_focusingUis.FirstOrDefault(t => t == view) != default)
                    {
                        _focusingUis.Remove(view);

                        if (_focusingUis.Any() == false)
                        {
                            //GlobalAPI.AppEnv.UIFocusState = EUIFocusState.None;
                        }
                    }
                // Debug.Log($"ViewRootBehaviour.Close : {behaviour.GetType().Name}");
            }
        }

        public GameObject FindUiObject<T>(string path = default) where T : MonoBehaviour
        {
            var prefabPath = GetPrefabName(typeof(T), path);
            if (_cahcedPrefabDictionary.TryGetValue(prefabPath, out var prefab)) return prefab;

            return default;
        }

        public GameObject FindOrCreateUiObject<T>(string path = default) where T : MonoBehaviour
        {
            if (FindUiObject<T>(path) is var findGameObject && findGameObject == default)
            {
                if (CreateViewObject(typeof(T), path) is var createdGameObject && createdGameObject != default)
                    return createdGameObject;
            }
            else
            {
                return findGameObject;
            }

            return default;
        }

        public T GetOrAddComponent<T>(string path = default) where T : MonoBehaviour
        {
            return FindOrCreateUiObject<T>(path)?.GetOrAddComponent<T>();
        }

        private void SortUiElementInLayer(Transform parent)
        {
            List<(MonoBehaviour View, int Order)> sortingViewList = new();
            for (var i = 0; i < parent.childCount; ++i)
            {
                var monoBehaviours = parent.GetChild(i).GetComponents<MonoBehaviour>();
                if (monoBehaviours.Any())
                {
                    var childMonoBehaviour = monoBehaviours.FirstOrDefault(t => t is IUiView);
                    if (childMonoBehaviour != default)
                    {
                        var order = GetOrder(childMonoBehaviour.GetType());
                        if (order != null) sortingViewList.Add((childMonoBehaviour, (int)order));
                    }
                }
            }

            sortingViewList.Sort(SortByOrder);

            for (var i = 0; i < sortingViewList.Count; ++i) sortingViewList[i].View.transform.SetAsLastSibling();
        }

        private void Prepare(GameObject go, Type type)
        {
            SetLayer(go, type);
        }

        public void EnableUI(bool enabled)
        {
            foreach (var canvas in AllLayer.Select(t => t.transform.parent.GetComponent<Canvas>()))
                canvas.enabled = enabled;
        }
    }
}