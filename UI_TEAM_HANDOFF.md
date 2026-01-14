# 📤 UI Ekibine İletilecek Bilgiler

## 🎯 Proje Özeti

Turkcell Campaign Optimizer Backend API'si tamamlandı ve frontend entegrasyonu için hazır.

---

## 📚 İletilecek Dosyalar

### 1. **API_DOCUMENTATION.md** ⭐ (EN ÖNEMLİ)
**İçerik:**
- Tüm API endpoint'lerinin detaylı açıklaması
- Request/Response örnekleri
- React/Axios kullanım örnekleri
- Error handling
- CORS bilgileri

**Neden Önemli:** Frontend geliştiricilerin ihtiyaç duyduğu tüm teknik bilgiler burada.

### 2. **README.md**
**İçerik:**
- Proje genel bakış
- Kurulum adımları
- Proje yapısı
- Test senaryoları
- Troubleshooting

### 3. **PROJECT_STATUS.md**
**İçerik:**
- Tamamlanan tüm özellikler
- Test senaryoları
- Beklenen sonuçlar
- Entegrasyon örnekleri

---

## 🌐 Backend Bilgileri

### Base URL
```
http://localhost:5000/api
```

### CORS Ayarları
Backend, aşağıdaki origin'lerden gelen istekleri kabul eder:
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite default)
- `http://localhost:4200` (Angular default)

**Not:** Farklı bir port kullanılacaksa backend ekibine bildirilmeli.

---

## 📊 API Endpoint'leri Hızlı Referans

### Dashboard
```
GET /api/dashboard/summary
```
Tüm özet kartları için tek endpoint.

### Users
```
GET /api/users?page=1&pageSize=20&segment=HIGH_USAGE
GET /api/users/{userId}
```

### Campaigns
```
GET /api/campaigns?page=1&pageSize=20
```

### Assignments
```
POST /api/assign
Body: { "userId": "U003" }

PATCH /api/status-update
Body: { "assignmentId": 1, "newStatus": "USED" }

GET /api/assignments?page=1&pageSize=20&status=ASSIGNED
```

---

## 🎨 Frontend Gereksinimler

### Gerekli Sayfalar/Bileşenler

#### 1. Dashboard (Ana Sayfa)
**Gösterilecek Metrikler:**
- Total Users
- Active Users
- Total Campaigns
- Active Campaigns
- Total Assignments
- Success Rate (%)
- Average Score

**API Endpoint:** `GET /api/dashboard/summary`

#### 2. Users Listesi
**Özellikler:**
- Sayfalama (pagination)
- Segment filtreleme
- Kullanıcı detay görüntüleme
- Kampanya atama butonu

**API Endpoints:**
- `GET /api/users?page=1&pageSize=20`
- `GET /api/users/{userId}`
- `POST /api/assign`

#### 3. Campaigns Listesi
**Özellikler:**
- Sayfalama
- Aktif/Pasif göstergesi
- Öncelik göstergesi
- Atama sayısı

**API Endpoint:** `GET /api/campaigns?page=1&pageSize=20`

#### 4. Assignments Listesi
**Özellikler:**
- Sayfalama
- Status filtreleme (ASSIGNED, USED, EXPIRED)
- Durum güncelleme
- Kullanıcı ve kampanya bilgileri

**API Endpoints:**
- `GET /api/assignments?page=1&pageSize=20`
- `PATCH /api/status-update`

---

## 💻 Örnek React Implementasyonu

### API Service Dosyası
```javascript
// services/api.js
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

export const api = {
  // Dashboard
  getDashboardSummary: () => 
    axios.get(`${API_BASE_URL}/dashboard/summary`),

  // Users
  getUsers: (page = 1, pageSize = 20, segment = null) => 
    axios.get(`${API_BASE_URL}/users`, { 
      params: { page, pageSize, segment } 
    }),
  
  getUserById: (userId) => 
    axios.get(`${API_BASE_URL}/users/${userId}`),

  // Campaigns
  getCampaigns: (page = 1, pageSize = 20) => 
    axios.get(`${API_BASE_URL}/campaigns`, { 
      params: { page, pageSize } 
    }),

  // Assignments
  assignCampaign: (userId) => 
    axios.post(`${API_BASE_URL}/assign`, { userId }),

  updateAssignmentStatus: (assignmentId, newStatus) => 
    axios.patch(`${API_BASE_URL}/status-update`, { 
      assignmentId, 
      newStatus 
    }),

  getAssignments: (page = 1, pageSize = 20, status = null) => 
    axios.get(`${API_BASE_URL}/assignments`, { 
      params: { page, pageSize, status } 
    }),
};
```

### Dashboard Component Örneği
```jsx
import React, { useState, useEffect } from 'react';
import { api } from './services/api';

function Dashboard() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const response = await api.getDashboardSummary();
      setSummary(response.data);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="dashboard">
      <h1>Campaign Optimizer Dashboard</h1>
      <div className="stats-grid">
        <StatCard title="Total Users" value={summary.totalUsers} />
        <StatCard title="Active Campaigns" value={summary.activeCampaigns} />
        <StatCard title="Success Rate" value={`${summary.successRate}%`} />
        <StatCard title="Average Score" value={summary.averageScore} />
      </div>
    </div>
  );
}
```

---

## 🧪 Test Senaryosu: U003 Kullanıcısı

### Adım 1: Kullanıcı Bilgilerini Görüntüle
```javascript
const user = await api.getUserById('U003');
```

**Beklenen Sonuç:**
```json
{
  "userId": "U003",
  "name": "Mehmet Kaya",
  "city": "Izmir",
  "segment": "HIGH_USAGE",
  "monthlyDataGb": 60.0,
  "monthlySpendTry": 450.00,
  "loyaltyYears": 8,
  "score": 47.06
}
```

### Adım 2: Kampanya Ata
```javascript
const result = await api.assignCampaign('U003');
```

**Beklenen Sonuç:**
```json
{
  "success": true,
  "message": "Campaign assigned successfully",
  "assignment": {
    "assignmentId": 1,
    "userId": "U003",
    "campaignId": "C001",
    "campaignType": "DATA_BOOST",
    "score": 47.06,
    "status": "ASSIGNED"
  }
}
```

### Adım 3: Durumu Güncelle
```javascript
const result = await api.updateAssignmentStatus(1, 'USED');
```

**Beklenen Sonuç:**
```json
{
  "success": true,
  "message": "Campaign status updated successfully"
}
```

---

## 📋 UI Tasarım Önerileri

### Dashboard Kartları
```
┌─────────────────────┐  ┌─────────────────────┐
│   Total Users       │  │  Active Campaigns   │
│       10            │  │         7           │
└─────────────────────┘  └─────────────────────┘

┌─────────────────────┐  ┌─────────────────────┐
│   Success Rate      │  │   Average Score     │
│      78.5%          │  │       65.3          │
└─────────────────────┘  └─────────────────────┘
```

### Users Table
```
┌────────┬──────────────┬──────────┬──────────────┬────────┬─────────┐
│ User ID│ Name         │ City     │ Segment      │ Score  │ Actions │
├────────┼──────────────┼──────────┼──────────────┼────────┼─────────┤
│ U003   │ Mehmet Kaya  │ Izmir    │ HIGH_USAGE   │ 47.06  │ [Assign]│
│ U001   │ Ahmet Yılmaz │ Istanbul │ HIGH_USAGE   │ 65.82  │ [Assign]│
└────────┴──────────────┴──────────┴──────────────┴────────┴─────────┘
```

### Campaigns Table
```
┌────────┬──────────────┬──────────────┬──────────┬────────┬────────────┐
│Camp ID │ Type         │ Segment      │ Priority │ Active │ Assignments│
├────────┼──────────────┼──────────────┼──────────┼────────┼────────────┤
│ C001   │ DATA_BOOST   │ HIGH_USAGE   │    1     │   ✓    │     5      │
│ C002   │ LOYALTY_RWD  │ HIGH_USAGE   │    2     │   ✓    │     3      │
└────────┴──────────────┴──────────────┴──────────┴────────┴────────────┘
```

---

## 🎯 Önemli Notlar

### 1. Pagination
Tüm liste endpoint'lerinde pagination kullanılmalı:
```javascript
// Sayfa değiştirme
const [page, setPage] = useState(1);
const [pageSize] = useState(20);

const fetchUsers = async () => {
  const response = await api.getUsers(page, pageSize);
  // response.pagination içinde sayfa bilgileri var
};
```

### 2. Error Handling
Her API çağrısında try-catch kullanılmalı:
```javascript
try {
  const response = await api.assignCampaign(userId);
  if (response.data.success) {
    // Başarılı
  } else {
    // Başarısız ama hata yok (örn: uygun kampanya yok)
  }
} catch (error) {
  // Gerçek hata (network, server error vs.)
  console.error('Error:', error);
}
```

### 3. Loading States
API çağrıları sırasında loading göstergesi gösterilmeli:
```javascript
const [loading, setLoading] = useState(false);

const handleAssign = async () => {
  setLoading(true);
  try {
    await api.assignCampaign(userId);
  } finally {
    setLoading(false);
  }
};
```

### 4. Real-time Updates
Bir işlem sonrası ilgili verileri yeniden yükleyin:
```javascript
const handleAssign = async (userId) => {
  await api.assignCampaign(userId);
  // Dashboard'u güncelle
  await fetchDashboardSummary();
  // Assignments listesini güncelle
  await fetchAssignments();
};
```

---

## 🔧 Backend Başlatma

UI ekibinin backend'i çalıştırması için:

```bash
cd c:\Users\ozkar\OneDrive\Masaüstü\case4
dotnet run
```

API şu adreste çalışacaktır: `http://localhost:5000`

---

## 📞 Destek ve İletişim

### Sorular için Kontrol Edilecek Dosyalar:
1. **API_DOCUMENTATION.md** - API kullanımı
2. **README.md** - Genel bilgiler
3. **PROJECT_STATUS.md** - Tamamlanan özellikler

### Backend Logları:
```
c:\Users\ozkar\OneDrive\Masaüstü\case4\logs\
```

---

## ✅ Checklist - UI Ekibi İçin

- [ ] API_DOCUMENTATION.md dosyasını oku
- [ ] Backend'i `dotnet run` ile başlat
- [ ] `http://localhost:5000/api/dashboard/summary` endpoint'ini test et
- [ ] Axios kurulumunu yap (`npm install axios`)
- [ ] API service dosyasını oluştur
- [ ] Dashboard component'ini oluştur
- [ ] Users listesi component'ini oluştur
- [ ] Campaigns listesi component'ini oluştur
- [ ] Assignments listesi component'ini oluştur
- [ ] U003 kullanıcısı ile test et
- [ ] Pagination'ı implement et
- [ ] Error handling ekle
- [ ] Loading states ekle

---

## 🎉 Özet

✅ **Backend Hazır**
✅ **API Dokümante Edildi**
✅ **Test Senaryoları Hazır**
✅ **Örnek Kodlar Sağlandı**
✅ **CORS Yapılandırıldı**

**UI ekibi artık frontend geliştirmeye başlayabilir!**

---

**Hazırlayan:** Backend Team
**Tarih:** 14 Ocak 2026
**Durum:** ✅ Ready for Frontend Integration
