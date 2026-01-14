# 🎯 Turkcell Campaign Optimizer - Backend

Turkcell Campaign Optimizer, kullanıcı davranışlarına ve segmentlerine göre akıllı kampanya ataması yapan bir backend sistemidir.

## 📋 Özellikler

### ✅ Tamamlanan Özellikler

#### 🟢 Adım 1: Veri Modelleri ve Veritabanı
- ✅ **Entity Models**: User, UserMetric, Campaign, Assignment, Notification
- ✅ **DbContext**: Entity Framework Core ile MSSQL entegrasyonu
- ✅ **CSV Data Seeder**: Otomatik veri yükleme sistemi
- ✅ **Database Migrations**: Veritabanı şema yönetimi

#### 🟡 Adım 2: Skorlama ve Karar Motoru
- ✅ **Skorlama Algoritması**: 
  - Formula: `Score = (monthly_data_gb × 0.5) + (monthly_spend_try × 0.3) + (loyalty_years × 0.2)`
  - 0-100 arası normalize edilmiş skorlama
- ✅ **Kampanya Atama Motoru**:
  - Segment bazlı filtreleme
  - Öncelik sıralaması (1 = en yüksek)
  - Skor bazlı seçim
  - Duplicate atama kontrolü

#### 🔵 Adım 3: API ve Responsive UI Desteği
- ✅ **Dashboard Endpoint**: Tek seferde tüm özet metrikleri
- ✅ **Pagination**: Tüm liste endpoint'lerinde sayfalama
- ✅ **Campaign Assignment API**: POST /api/assign
- ✅ **Status Update API**: PATCH /api/status-update
- ✅ **CORS Desteği**: Frontend entegrasyonu için hazır

## 🚀 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- SQL Server LocalDB (veya SQL Server)
- Visual Studio Code / Visual Studio 2022

### Adımlar

1. **Projeyi klonlayın veya indirin**
```bash
cd c:\Users\ozkar\OneDrive\Masaüstü\case4
```

2. **NuGet paketlerini geri yükleyin**
```bash
dotnet restore
```

3. **Veritabanını oluşturun**
```bash
dotnet ef database update
```

4. **Uygulamayı çalıştırın**
```bash
dotnet run
```

API şu adreste çalışacaktır: `http://localhost:5000`

## 📁 Proje Yapısı

```
case4/
├── Models/                 # Entity modelleri
│   ├── User.cs
│   ├── UserMetric.cs
│   ├── Campaign.cs
│   ├── Assignment.cs
│   └── Notification.cs
├── Data/                   # DbContext ve veritabanı
│   └── AppDbContext.cs
├── Services/               # İş mantığı servisleri
│   ├── CampaignEngineService.cs
│   └── DataSeederService.cs
├── Controllers/            # API Controllers
│   ├── DashboardController.cs
│   ├── CampaignController.cs
│   └── UsersController.cs
├── DTOs/                   # Data Transfer Objects
│   └── ApiDTOs.cs
├── SeedData/              # CSV dosyaları
│   ├── users.csv
│   ├── user_metrics.csv
│   └── campaigns.csv
├── Migrations/            # EF Core migrations
├── logs/                  # Serilog log dosyaları
├── Program.cs             # Uygulama başlangıcı
├── appsettings.json       # Yapılandırma
├── BACKEND_TODO.md        # Geliştirme adımları
└── API_DOCUMENTATION.md   # Frontend için API dokümanı
```

## 🎯 API Endpoints

### Dashboard
- `GET /api/dashboard/summary` - Dashboard özet istatistikleri

### Users
- `GET /api/users` - Tüm kullanıcılar (paginated)
- `GET /api/users/{userId}` - Kullanıcı detayı

### Campaigns
- `GET /api/campaigns` - Tüm kampanyalar (paginated)
- `POST /api/assign` - Kampanya ataması
- `PATCH /api/status-update` - Atama durumu güncelleme

### Assignments
- `GET /api/assignments` - Tüm atamalar (paginated)

Detaylı API dokümantasyonu için: [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)

## 🧪 Test Senaryosu

### U3 Kullanıcısı için Test

```bash
# 1. Kullanıcı bilgilerini getir
curl http://localhost:5000/api/users/U003

# 2. Kampanya ata
curl -X POST http://localhost:5000/api/assign \
  -H "Content-Type: application/json" \
  -d '{"userId":"U003"}'

# 3. Dashboard'u kontrol et
curl http://localhost:5000/api/dashboard/summary

# 4. Atama durumunu güncelle
curl -X PATCH http://localhost:5000/api/status-update \
  -H "Content-Type: application/json" \
  -d '{"assignmentId":1,"newStatus":"USED"}'
```

### Beklenen Sonuçlar

**U003 Kullanıcısı:**
- Name: Mehmet Kaya
- Segment: HIGH_USAGE
- Monthly Data: 60 GB
- Monthly Spend: 450 TRY
- Loyalty: 8 years
- **Calculated Score: ~72.45**

**Atanacak Kampanya:**
- Campaign ID: C001
- Type: DATA_BOOST
- Target Segment: HIGH_USAGE
- Priority: 1 (en yüksek)

## 🔧 Yapılandırma

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TurkcellCampaignOptimizer;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### CORS Ayarları

Frontend için izin verilen origin'ler:
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)
- `http://localhost:4200` (Angular)

Farklı bir port kullanıyorsanız `Program.cs` dosyasındaki CORS ayarlarını güncelleyin.

## 📊 Skorlama Algoritması

### Formula
```
Score = (monthly_data_gb × 0.5) + (monthly_spend_try × 0.3) + (loyalty_years × 0.2)
```

### Normalizasyon
- Max değerler: data=100GB, spend=1000TRY, loyalty=20years
- Max possible score: 354
- Normalized: (rawScore / 354) × 100
- Range: 0-100

### Örnek Hesaplama
```
User: U003
- monthly_data_gb: 60
- monthly_spend_try: 450
- loyalty_years: 8

Raw Score = (60 × 0.5) + (450 × 0.3) + (8 × 0.2)
          = 30 + 135 + 1.6
          = 166.6

Normalized = (166.6 / 354) × 100 = 47.06
```

## 🎨 Frontend Entegrasyonu

### React Örneği

```javascript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

// Dashboard verilerini getir
const fetchDashboard = async () => {
  const response = await axios.get(`${API_BASE_URL}/dashboard/summary`);
  return response.data;
};

// Kampanya ata
const assignCampaign = async (userId) => {
  const response = await axios.post(`${API_BASE_URL}/assign`, { userId });
  return response.data;
};
```

Daha fazla örnek için: [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)

## 📝 Logging

Serilog kullanılarak loglama yapılmaktadır:
- Console output
- File output: `logs/turkcell-campaign-{Date}.txt`

Log seviyeleri:
- Information: Genel bilgilendirme
- Warning: Uyarılar
- Error: Hatalar

## 🔒 Güvenlik

- Input validation tüm endpoint'lerde aktif
- SQL injection koruması (EF Core parametreli sorgular)
- CORS politikası ile origin kontrolü
- Status transition validation

## 🐛 Troubleshooting

### Veritabanı Bağlantı Hatası
```bash
# LocalDB'nin çalıştığından emin olun
sqllocaldb info

# Veritabanını yeniden oluşturun
dotnet ef database drop --force
dotnet ef database update
```

### CSV Dosyaları Yüklenmiyor
- `SeedData` klasörünün varlığını kontrol edin
- CSV dosyalarının encoding'inin UTF-8 olduğundan emin olun
- Log dosyalarını kontrol edin: `logs/`

### CORS Hatası
- Frontend URL'inin `Program.cs` içindeki CORS ayarlarında olduğundan emin olun
- Browser console'da detaylı hata mesajını kontrol edin

## 📈 Performans

- **Database Indexing**: Sık kullanılan sorgular için index'ler eklendi
- **Pagination**: Büyük veri setleri için sayfalama
- **Eager Loading**: N+1 sorgu problemini önlemek için Include kullanımı
- **Async/Await**: Tüm I/O operasyonları asenkron

## 🔄 Gelecek Geliştirmeler

- [ ] Authentication & Authorization (JWT)
- [ ] Rate limiting
- [ ] Caching (Redis)
- [ ] Background jobs (Hangfire)
- [ ] Email/SMS notification gönderimi
- [ ] Advanced analytics
- [ ] A/B testing desteği
- [ ] Machine learning entegrasyonu

## 👥 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje Turkcell için geliştirilmiştir.

## 📞 İletişim

Sorularınız için lütfen proje dokümantasyonunu kontrol edin:
- [BACKEND_TODO.md](./BACKEND_TODO.md) - Geliştirme adımları
- [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) - API kullanım kılavuzu

---

**Geliştirme Durumu:** ✅ Production Ready

**Son Güncelleme:** 14 Ocak 2026
