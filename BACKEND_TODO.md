# Turkcell Campaign Optimizer - Backend Master To-Do

Bu dosya, Turkcell Campaign Optimizer projesinin backend geliştirme adımlarını içerir. Cursor IDE'ye "Bu dosyadaki adımları sırayla takip et" komutu vererek sistematik bir şekilde ilerleyebilirsiniz.

---

## 🟢 1. Adım: Veri Modelleri ve Veritabanı (Mimar)

### 1.1 Modelleri Oluştur
Paylaşılan CSV dosyalarındaki sütun yapılarına uygun C# sınıflarını (Entities) oluştur:

#### 1.1.1 User.cs
```
Özellikler:
- user_id (string, Primary Key)
- name (string)
- city (string)
- segment (string)
```

#### 1.1.2 UserMetric.cs
```
Özellikler:
- id (int, Primary Key, Auto-increment)
- user_id (string, Foreign Key -> User)
- monthly_data_gb (decimal)
- monthly_spend_try (decimal)
- loyalty_years (int)
```

#### 1.1.3 Campaign.cs
```
Özellikler:
- campaign_id (string, Primary Key)
- type (string)
- target_segment (string)
- priority (int) - Not: 1 en yüksek önceliktir
- start_date (DateTime)
- end_date (DateTime)
- is_active (bool)
```

#### 1.1.4 Assignment.cs
```
Özellikler:
- assignment_id (int, Primary Key, Auto-increment)
- user_id (string, Foreign Key -> User)
- campaign_id (string, Foreign Key -> Campaign)
- score (decimal) - Hesaplanan uygunluk skoru
- status (string) - ASSIGNED, USED, EXPIRED
- assigned_at (DateTime)
```

#### 1.1.5 Notification.cs
```
Özellikler:
- notification_id (int, Primary Key, Auto-increment)
- user_id (string, Foreign Key -> User)
- channel (string) - SMS, EMAIL, PUSH
- message (string)
- sent_at (DateTime)
```

### 1.2 DbContext Yapılandırması
- `AppDbContext` sınıfını oluştur
- Tüm entity'leri DbSet olarak ekle
- `appsettings.json` içerisine MSSQL `DefaultConnection` connection string'ini ekle
- `OnModelCreating` metodunda ilişkileri ve kısıtlamaları tanımla

### 1.3 CSV Data Seeder
Uygulama ayağa kalktığında aşağıdaki CSV dosyalarını okuyup MSSQL veritabanını dolduran bir servis yaz:
- `users.csv`
- `user_metrics.csv`
- `campaigns.csv`

**Gereksinimler:**
- Uygulama başlangıcında otomatik çalışmalı
- Duplicate kayıt kontrolü yapmalı
- Hata durumunda loglama yapmalı

---

## 🟡 2. Adım: Skorlama ve Karar Motoru (Beyin)

### 2.1 Skorlama Metodu (The Formula)
`UserMetric` verilerini kullanarak aşağıdaki matematiksel modeli kodla:

**Formül:**
```
Score = (monthly_data_gb × 0.5) + (monthly_spend_try × 0.3) + (loyalty_years × 0.2)
```

**Cursor Promptu:**
```
Veri tabanındaki UserMetric tablosunu kullan. Her kullanıcı için monthly_data_gb ağırlığını 0.5, 
monthly_spend_try ağırlığını 0.3 ve loyalty_years ağırlığını 0.2 alarak 0-100 arasında normalize 
edilmiş bir CalculateSuitabilityScore metodu oluştur. Bu metot, CampaignAssignment tablosuna kayıt 
atarken score sütununa yazılacak değeri üretmeli.
```

**Metod İmzası:**
```csharp
public decimal CalculateSuitabilityScore(UserMetric userMetric)
```

**Çıktı:**
- 0-100 arasında normalize edilmiş skor değeri
- Decimal tipinde hassas hesaplama

### 2.2 Kampanya Atama Motoru

**Algoritma Adımları:**

1. **Segment Filtreleme:**
   - Kullanıcının segmentine (Örn: HIGH_USAGE, MEDIUM_USAGE, LOW_USAGE) uyan kampanyaları filtrele
   - Sadece aktif kampanyaları (`is_active = true`) dikkate al
   - Tarih aralığı kontrolü yap (`start_date <= NOW <= end_date`)

2. **Öncelik Sıralama:**
   - Filtrelenen kampanyalar arasından en yüksek önceliğe (`priority`) sahip olanı seç
   - **Not:** 1 en yüksek önceliktir (1 > 2 > 3...)

3. **Skor Bazlı Seçim:**
   - Öncelikler eşitse, hesaplanan en yüksek `Score` değerine sahip kampanyayı kullanıcıya ata
   - Eşitlik durumunda kampanya oluşturma tarihine göre sırala (en yeni önce)

4. **Atama Kaydı:**
   - Seçilen kampanyayı `Assignment` tablosuna kaydet
   - İlk durum: `status = "ASSIGNED"`
   - `assigned_at` alanını şu anki zaman ile doldur

**Metod İmzası:**
```csharp
public async Task<Assignment> AssignCampaignToUser(string userId)
```

---

## 🔵 3. Adım: API ve Responsive UI Desteği

### 3.1 Dashboard Özet Endpoint'i
UI arkadaşının tek seferde tüm "Özet Kartlarını" doldurabilmesi için bir endpoint oluştur.

**Endpoint:**
```
GET /api/dashboard/summary
```

**Response DTO (DashboardSummaryDTO):**
```csharp
{
    "totalUsers": 1000,
    "activeUsers": 850,
    "totalCampaigns": 15,
    "activeCampaigns": 8,
    "totalAssignments": 5420,
    "successRate": 78.5,
    "averageScore": 65.3
}
```

### 3.2 Sayfalama (Pagination)
Kullanıcı ve kampanya listelerini döndürürken `Skip` ve `Take` metodlarını kullanarak veriyi küçük parçalar halinde gönder.

**Pagination Parametreleri:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)

**Örnek Endpoint:**
```
GET /api/users?page=1&pageSize=20
GET /api/campaigns?page=1&pageSize=20
```

**Response Format:**
```csharp
{
    "data": [...],
    "pagination": {
        "currentPage": 1,
        "pageSize": 20,
        "totalItems": 1000,
        "totalPages": 50
    }
}
```

### 3.3 Aksiyon Kapıları (Action Endpoints)

#### 3.3.1 Kampanya Atama
```
POST /api/assign
```

**Request Body:**
```json
{
    "userId": "U001"
}
```

**Response:**
```json
{
    "success": true,
    "assignment": {
        "assignmentId": 123,
        "userId": "U001",
        "campaignId": "C001",
        "score": 85.5,
        "status": "ASSIGNED",
        "assignedAt": "2026-01-14T22:00:00Z"
    }
}
```

#### 3.3.2 Durum Güncelleme
```
PATCH /api/status-update
```

**Request Body:**
```json
{
    "assignmentId": 123,
    "newStatus": "USED"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Campaign status updated successfully"
}
```

**Geçerli Durum Geçişleri:**
- `ASSIGNED` → `USED`
- `ASSIGNED` → `EXPIRED`
- `USED` → (değiştirilemez)
- `EXPIRED` → (değiştirilemez)

---

## 📋 Ek Gereksinimler

### Loglama
- Her kritik işlem için loglama ekle (Serilog önerilir)
- Hata durumlarını detaylı logla
- API isteklerini logla

### Validasyon
- Tüm API endpoint'lerinde input validasyonu yap
- FluentValidation kullanımı önerilir
- Hatalı isteklerde açıklayıcı mesajlar dön

### Error Handling
- Global exception handler ekle
- Kullanıcı dostu hata mesajları dön
- HTTP status code'larını doğru kullan

### Testing
- Unit testler yaz (xUnit önerilir)
- Skorlama algoritmasını test et
- Kampanya atama mantığını test et
- Özellikle U3 kullanıcısı için test senaryoları oluştur

---

## 🎯 Öncelik Sırası

1. **Kritik (Önce Yapılmalı):**
   - Veri modelleri ve DbContext
   - CSV Data Seeder
   - Skorlama metodu
   - Kampanya atama motoru

2. **Yüksek (Hemen Sonra):**
   - Dashboard özet endpoint'i
   - Kampanya atama API'si
   - Durum güncelleme API'si

3. **Orta (Sonraki Aşama):**
   - Sayfalama implementasyonu
   - Loglama sistemi
   - Validasyon katmanı

4. **Düşük (İyileştirme):**
   - Unit testler
   - Performance optimizasyonları
   - Dokümantasyon

---

## 🚀 Cursor Kullanım İpuçları

1. **Modelleri oluştururken:**
   ```
   "Users.csv dosyasındaki sütunlara göre User.cs entity sınıfını oluştur. 
   Primary key, navigation properties ve data annotations ekle."
   ```

2. **DbContext için:**
   ```
   "AppDbContext sınıfını oluştur. User, UserMetric, Campaign, Assignment ve 
   Notification entity'lerini ekle. İlişkileri OnModelCreating'de tanımla."
   ```

3. **Seeder için:**
   ```
   "CSV dosyalarını okuyup veritabanına yazan bir DataSeeder servisi oluştur. 
   Duplicate kontrolü yap ve hata durumlarını logla."
   ```

4. **Skorlama için:**
   ```
   "CalculateSuitabilityScore metodunu oluştur. Formül: 
   (monthly_data_gb × 0.5) + (monthly_spend_try × 0.3) + (loyalty_years × 0.2). 
   Sonucu 0-100 arasında normalize et."
   ```

5. **API için:**
   ```
   "RESTful API controller oluştur. Dashboard özeti, kampanya atama ve 
   durum güncelleme endpoint'lerini ekle. Pagination desteği ekle."
   ```

---

## ✅ Tamamlanma Kriterleri

- [ ] Tüm entity sınıfları oluşturuldu
- [ ] DbContext yapılandırıldı ve migration'lar oluşturuldu
- [ ] CSV dosyaları başarıyla veritabanına yükleniyor
- [ ] Skorlama algoritması doğru çalışıyor
- [ ] Kampanya atama motoru segment ve öncelik kurallarına uyuyor
- [ ] Dashboard özet endpoint'i doğru veri dönüyor
- [ ] Kampanya atama API'si çalışıyor
- [ ] Durum güncelleme API'si çalışıyor
- [ ] Pagination tüm liste endpoint'lerinde çalışıyor
- [ ] U3 kullanıcısı için test senaryosu başarılı

---

**Not:** Bu adımları Cursor IDE'de sırayla takip ederek projenizi tamamlayabilirsiniz. Her adımda Cursor'a yukarıdaki promptları kullanarak detaylı talimatlar verebilirsiniz.
