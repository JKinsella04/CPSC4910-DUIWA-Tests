using DUIWA_Tests.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Security.Claims;
using Team01_DUIWA.Data;
using Team01_DUIWA.Models.StoreModels;

namespace DUIWA_Tests.Tests
{
    public class CatalogServiceTest
    {
        private readonly DUIWADbContext _context;
        private readonly HttpClient _httpClient;
        public CatalogServiceTest()
        {
            _context = CreateContext();
            _httpClient = new HttpClient();
        }

        private DUIWADbContext CreateContext()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<CatalogServiceTest>()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var options = new DbContextOptionsBuilder<DUIWADbContext>()
                .UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 44))
                )
                .Options;

            return new DUIWADbContext(options);
        }

        private HttpContextAccessor CreateHttpContextAccessor(int activeSponsorId, int sponsorIdClaim)
        {
            var claims = new List<Claim> { new Claim("SponsorId", sponsorIdClaim.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            var session = new MockSession();
            ((ISession)session).SetInt32("ActiveSponsorId", activeSponsorId);

            var httpContext = new DefaultHttpContext
            {
                User = principal,
                Session = session
            };

            return new HttpContextAccessor { HttpContext = httpContext };
        }

        private CatalogService CreateCatalogService(int activeSponsorId = 1, int sponsorIdClaim = 1)
        {
            var httpContextAccessor = CreateHttpContextAccessor(activeSponsorId, sponsorIdClaim);
            return new CatalogService(_httpClient, _context, httpContextAccessor);
        }

        [Fact]
        public async Task LookupArtist_ReturnsArtist()
        {
            var service = CreateCatalogService(activeSponsorId: 1, sponsorIdClaim: 1);

            // Lookup for DAY6
            var result = await service.LookupArtist("1037939997");

            Assert.NotNull(result);
            Assert.Equal("DAY6", result.artistName);
            Assert.Equal(1037939997, result.artistId);
            Assert.Equal("K-Pop", result.primaryGenreName);

        }

        [Fact]
        public async Task GetAllAlbums_ReturnsAlbums()
        {
            var service = CreateCatalogService(activeSponsorId: 1, sponsorIdClaim: 1);

            //Get all albums for sponsor 1, which should include DAY6
            var result = await service.GetAllAlbums();

            Assert.NotNull(result);
            Assert.Contains(result, album => album.artistName == "DAY6");
        }

        [Fact]
        public async Task GetAllAlbums_ReturnsNoAlbums()
        {
            //Sponsor 5 has no catalog
            var service = CreateCatalogService(activeSponsorId: 5, sponsorIdClaim: 1);

            var result = await service.GetAllAlbums();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task LookupAlbums_ReturnsAlbums()
        {
            var service = CreateCatalogService(activeSponsorId: 1, sponsorIdClaim: 1);
            
            var result = await service.LookupAlbums("1037939997");

            Assert.NotNull(result);
            Assert.Contains(result, album => album.collectionName == "The DECADE");
        }
    }
}
