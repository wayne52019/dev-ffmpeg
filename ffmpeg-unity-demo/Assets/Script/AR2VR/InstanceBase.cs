using UnityEngine;

namespace AR2VR {
    public abstract class InstanceBase<T> : MonoBehaviour where T : class
    {
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        static T _instance;

        /// <summary>
        /// 如果要覆寫記得要在繼承類執行SetInstance
        /// </summary>
        protected virtual void Awake()
        {
            SetInstance();
        }

        /// <summary>
        /// 設定Instance
        /// </summary>
        /// <param name="target"></param>
        protected void SetInstance()
        {
            if (_instance != null && _instance != this as T)
            {
                Debug.LogError("只能一個");
                Destroy(this);
                return;
            }

            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this as T)
            {
                Debug.Log("刪除了");

                _instance = null;
            }
        }
    }
}
