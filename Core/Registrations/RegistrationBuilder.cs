namespace DemContainer {
    public sealed class RegistrationBuilder {
        public readonly IContainerRegistrator containerRegistrator;
        public readonly FuncRegistrationRecord funcRegistrationRecord;

        public RegistrationBuilder(IContainerRegistrator containerRegistrator, FuncRegistrationRecord funcRegistrationRecord) {
            this.containerRegistrator = containerRegistrator;
            this.funcRegistrationRecord = funcRegistrationRecord;
        }

        public RegistrationBuilder Lazy() {
            funcRegistrationRecord.Lazy = true;
            return this;
        }
    }
}