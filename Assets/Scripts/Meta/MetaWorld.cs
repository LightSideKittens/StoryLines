using System;
using LSCore;

namespace Meta
{
    [Serializable]
    public class LoadMetaScene : LSAction
    {
        public override void Invoke()
        {
            Common.LoadScene.metaLoadScene.Invoke();
        }
    }
    
    public class MetaWorld : ServiceManager
    {
        public Common.LoadScene metaLoadScene;

        protected override void Awake()
        {
            base.Awake();
            Common.LoadScene.metaLoadScene = metaLoadScene;
            MainWindow.Show();
        }
    }
}