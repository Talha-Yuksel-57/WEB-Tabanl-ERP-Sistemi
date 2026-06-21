using AutoMapper;
using ERP.Core.Mappings;

namespace ERP.Tests.Helpers
{
    /// <summary>
    /// Testlerde gerçek AutoMapper instance'ı kullanmak için yardımcı sınıf.
    /// Mock yerine gerçek mapper kullanıyoruz — mapping hataları da yakalanır.
    /// </summary>
    public static class MapperHelper
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MappingProfile>());

            config.AssertConfigurationIsValid(); // Mapping hataları compile-time yakalanır
            return config.CreateMapper();
        }
    }
}
