using System;

namespace StoryWorkshop
{
    [Serializable]
    public class SetBranchAction : BaseStoryAction
    {
        public string branchId;
        [NonSerialized] public string lastBranchId;
        
        protected override bool ShouldSaveAction => false;

        public override void Preload()
        {
            base.Preload();
            if(StoryWorld.TryGetLastBranchId(id, out var lastId))
            {
                lastBranchId = lastId;
            }
        }
        
        protected override void OnInvoke()
        {
            lastBranchId = StoryBranch.ActiveBranchId;
            StoryWorld.SetLastBranchId(id, lastBranchId);
            SetBranch(branchId, this);
        }

        private void SetBranch(string id, SetBranchAction backAction)
        {
            if (StoryWorld.SceneByBranchId.TryGetValue(id, out var loadSceneAction))
            {
                loadSceneAction.onSuccess.Clear();
                loadSceneAction.onSuccess.Add((DelegateLSAction)Set);
                loadSceneAction.Invoke();
                return;
            }

            Set();
            void Set()
            {
                if (backAction != null)
                {
                    StoryBranch.SetActiveBranch(id, backAction);
                }
                else
                {
                    StoryBranch.SetActiveBranch(id);
                }
            }
        }
        
        public override void Discard()
        {
            base.Discard();
            SetBranch(lastBranchId, null);
        }
    }
}