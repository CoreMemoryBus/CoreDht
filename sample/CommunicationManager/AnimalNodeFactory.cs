namespace CommunicationManager
{
    public class AnimalNodeFactory
    {
        private readonly NodeServicesFactory _servicesFactory;

        public AnimalNodeFactory(NodeServicesFactory servicesFactory)
        {
            _servicesFactory = servicesFactory;
        }

        /// <summary>
        /// Strictly Inproc implementation at the moment.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public AnimalNode CreateNode(string identifier)
        {
            return new AnimalNode(identifier, identifier, _servicesFactory.CreateServices());
        }
    }
}