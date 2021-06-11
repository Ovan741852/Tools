namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Custom Input Module")]
    [RequireComponent(typeof(ForceTouchInput))]
    public class CustomInputModule : StandaloneInputModule
    {
        public ForceTouchInput customInput;

        protected override void Awake()
        {
            base.Awake();
            inputOverride = customInput;
        }

        public override void Process()
        {
            customInput.BeforeProcess();
            base.Process();
            customInput.AfterProcess();
        }
    }
}
