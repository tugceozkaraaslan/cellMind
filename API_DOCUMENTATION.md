# Turkcell Campaign Optimizer - API Documentation for Frontend

Bu doküman, frontend geliştiriciler için backend API endpoint'lerini ve kullanım örneklerini içerir.

## Base URL
```
http://localhost:5000/api
```

## CORS Ayarları
Backend, aşağıdaki origin'lerden gelen istekleri kabul eder:
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite default)
- `http://localhost:4200` (Angular default)

---

## 📊 Dashboard Endpoints

### GET /api/dashboard/summary
Dashboard için özet istatistikleri getirir.

**Response:**
```json
{
  "totalUsers": 10,
  "activeUsers": 8,
  "totalCampaigns": 8,
  "activeCampaigns": 7,
  "totalAssignments": 15,
  "successRate": 78.5,
  "averageScore": 65.3
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const fetchDashboardSummary = async () => {
  const response = await axios.get('http://localhost:5000/api/dashboard/summary');
  return response.data;
};
```

---

## 👥 User Endpoints

### GET /api/users
Tüm kullanıcıları sayfalama ile getirir.

**Query Parameters:**
- `page` (int, default: 1) - Sayfa numarası
- `pageSize` (int, default: 20, max: 100) - Sayfa başına kayıt sayısı
- `segment` (string, optional) - Segment filtreleme (HIGH_USAGE, MEDIUM_USAGE, LOW_USAGE)

**Response:**
```json
{
  "data": [
    {
      "userId": "U001",
      "name": "Ahmet Yılmaz",
      "city": "Istanbul",
      "segment": "HIGH_USAGE",
      "monthlyDataGb": 45.5,
      "monthlySpendTry": 350.00,
      "loyaltyYears": 5,
      "score": 65.82
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 10,
    "totalPages": 1
  }
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const fetchUsers = async (page = 1, pageSize = 20, segment = null) => {
  const params = { page, pageSize };
  if (segment) params.segment = segment;
  
  const response = await axios.get('http://localhost:5000/api/users', { params });
  return response.data;
};
```

### GET /api/users/{userId}
Belirli bir kullanıcının detaylarını getirir.

**Response:**
```json
{
  "userId": "U003",
  "name": "Mehmet Kaya",
  "city": "Izmir",
  "segment": "HIGH_USAGE",
  "monthlyDataGb": 60.0,
  "monthlySpendTry": 450.00,
  "loyaltyYears": 8,
  "score": 72.45
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const fetchUserById = async (userId) => {
  const response = await axios.get(`http://localhost:5000/api/users/${userId}`);
  return response.data;
};
```

---

## 🎯 Campaign Endpoints

### GET /api/campaigns
Tüm kampanyaları sayfalama ile getirir.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)

**Response:**
```json
{
  "data": [
    {
      "campaignId": "C001",
      "type": "DATA_BOOST",
      "targetSegment": "HIGH_USAGE",
      "priority": 1,
      "startDate": "2026-01-01T00:00:00Z",
      "endDate": "2026-12-31T23:59:59Z",
      "isActive": true,
      "assignmentCount": 5
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 8,
    "totalPages": 1
  }
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const fetchCampaigns = async (page = 1, pageSize = 20) => {
  const response = await axios.get('http://localhost:5000/api/campaigns', {
    params: { page, pageSize }
  });
  return response.data;
};
```

---

## 📝 Assignment Endpoints

### POST /api/assign
Bir kullanıcıya kampanya atar.

**Request Body:**
```json
{
  "userId": "U003"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Campaign assigned successfully",
  "assignment": {
    "assignmentId": 1,
    "userId": "U003",
    "campaignId": "C001",
    "score": 72.45,
    "status": "ASSIGNED",
    "assignedAt": "2026-01-14T19:10:00Z",
    "userName": "Mehmet Kaya",
    "campaignType": "DATA_BOOST"
  }
}
```

**Response (No Campaign Found):**
```json
{
  "success": false,
  "message": "No eligible campaign found for this user",
  "assignment": null
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const assignCampaign = async (userId) => {
  const response = await axios.post('http://localhost:5000/api/assign', {
    userId: userId
  });
  return response.data;
};

// Kullanım
const result = await assignCampaign('U003');
if (result.success) {
  console.log('Campaign assigned:', result.assignment);
} else {
  console.log('Error:', result.message);
}
```

### PATCH /api/status-update
Atama durumunu günceller.

**Request Body:**
```json
{
  "assignmentId": 1,
  "newStatus": "USED"
}
```

**Valid Status Values:**
- `USED` - Kampanya kullanıldı
- `EXPIRED` - Kampanya süresi doldu

**Response (Success):**
```json
{
  "success": true,
  "message": "Campaign status updated successfully"
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Failed to update status. Assignment may not exist or status transition is invalid."
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const updateAssignmentStatus = async (assignmentId, newStatus) => {
  const response = await axios.patch('http://localhost:5000/api/status-update', {
    assignmentId: assignmentId,
    newStatus: newStatus
  });
  return response.data;
};

// Kullanım
const result = await updateAssignmentStatus(1, 'USED');
if (result.success) {
  console.log('Status updated successfully');
}
```

### GET /api/assignments
Tüm atamaları sayfalama ile getirir.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `status` (string, optional) - Durum filtreleme (ASSIGNED, USED, EXPIRED)

**Response:**
```json
{
  "data": [
    {
      "assignmentId": 1,
      "userId": "U003",
      "campaignId": "C001",
      "score": 72.45,
      "status": "ASSIGNED",
      "assignedAt": "2026-01-14T19:10:00Z",
      "userName": "Mehmet Kaya",
      "campaignType": "DATA_BOOST"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 15,
    "totalPages": 1
  }
}
```

**Kullanım Örneği (React/Axios):**
```javascript
const fetchAssignments = async (page = 1, pageSize = 20, status = null) => {
  const params = { page, pageSize };
  if (status) params.status = status;
  
  const response = await axios.get('http://localhost:5000/api/assignments', { params });
  return response.data;
};
```

---

## 🎨 Frontend Integration Examples

### React Component Example

```jsx
import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

function Dashboard() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/dashboard/summary`);
      setSummary(response.data);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="dashboard">
      <h1>Campaign Optimizer Dashboard</h1>
      <div className="stats">
        <div className="stat-card">
          <h3>Total Users</h3>
          <p>{summary.totalUsers}</p>
        </div>
        <div className="stat-card">
          <h3>Active Campaigns</h3>
          <p>{summary.activeCampaigns}</p>
        </div>
        <div className="stat-card">
          <h3>Success Rate</h3>
          <p>{summary.successRate}%</p>
        </div>
        <div className="stat-card">
          <h3>Average Score</h3>
          <p>{summary.averageScore}</p>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
```

### User Assignment Example

```jsx
import React, { useState } from 'react';
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

function UserAssignment() {
  const [userId, setUserId] = useState('');
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleAssign = async () => {
    setLoading(true);
    try {
      const response = await axios.post(`${API_BASE_URL}/assign`, {
        userId: userId
      });
      setResult(response.data);
    } catch (error) {
      console.error('Error assigning campaign:', error);
      setResult({ success: false, message: 'Error occurred' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="user-assignment">
      <h2>Assign Campaign to User</h2>
      <input
        type="text"
        placeholder="Enter User ID (e.g., U003)"
        value={userId}
        onChange={(e) => setUserId(e.target.value)}
      />
      <button onClick={handleAssign} disabled={loading}>
        {loading ? 'Assigning...' : 'Assign Campaign'}
      </button>

      {result && (
        <div className={`result ${result.success ? 'success' : 'error'}`}>
          <p>{result.message}</p>
          {result.assignment && (
            <div>
              <p>Campaign: {result.assignment.campaignType}</p>
              <p>Score: {result.assignment.score}</p>
              <p>Status: {result.assignment.status}</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default UserAssignment;
```

---

## 🔧 Error Handling

Tüm endpoint'ler aşağıdaki HTTP status code'larını kullanır:

- `200 OK` - İstek başarılı
- `400 Bad Request` - Geçersiz istek parametreleri
- `404 Not Found` - Kaynak bulunamadı
- `500 Internal Server Error` - Sunucu hatası

**Error Response Format:**
```json
{
  "message": "Error description"
}
```

---

## 🚀 Getting Started

1. Backend'i başlatın:
```bash
cd c:\Users\ozkar\OneDrive\Masaüstü\case4
dotnet run
```

2. API şu adreste çalışacaktır: `http://localhost:5000`

3. Frontend projenizde axios kurulumu:
```bash
npm install axios
```

4. API çağrıları için örnek servis dosyası oluşturun:

```javascript
// services/api.js
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

export const dashboardApi = {
  getSummary: () => axios.get(`${API_BASE_URL}/dashboard/summary`),
};

export const usersApi = {
  getAll: (page, pageSize, segment) => 
    axios.get(`${API_BASE_URL}/users`, { params: { page, pageSize, segment } }),
  getById: (userId) => 
    axios.get(`${API_BASE_URL}/users/${userId}`),
};

export const campaignsApi = {
  getAll: (page, pageSize) => 
    axios.get(`${API_BASE_URL}/campaigns`, { params: { page, pageSize } }),
  assign: (userId) => 
    axios.post(`${API_BASE_URL}/assign`, { userId }),
  updateStatus: (assignmentId, newStatus) => 
    axios.patch(`${API_BASE_URL}/status-update`, { assignmentId, newStatus }),
};

export const assignmentsApi = {
  getAll: (page, pageSize, status) => 
    axios.get(`${API_BASE_URL}/assignments`, { params: { page, pageSize, status } }),
};
```

---

## 📝 Test Data

Backend başlatıldığında otomatik olarak aşağıdaki test verileri yüklenir:

**Users:** U001 - U010 (10 kullanıcı)
- Segments: HIGH_USAGE, MEDIUM_USAGE, LOW_USAGE

**Campaigns:** C001 - C008 (8 kampanya)
- Her segment için farklı öncelik seviyelerinde kampanyalar

**Test için önerilen kullanıcı:** `U003` (Mehmet Kaya)
- Segment: HIGH_USAGE
- Yüksek skor değeri
- Kampanya ataması için ideal test kullanıcısı

---

## 💡 Tips

1. **Pagination:** Büyük veri setleri için mutlaka pagination kullanın
2. **Error Handling:** Tüm API çağrılarında try-catch kullanın
3. **Loading States:** API çağrıları sırasında loading göstergesi gösterin
4. **CORS:** Development sırasında farklı bir port kullanıyorsanız backend'de CORS ayarlarını güncelleyin
5. **Real-time Updates:** Assignment sonrası dashboard'u yenilemek için summary endpoint'ini tekrar çağırın

---

## 📞 Support

Herhangi bir sorun yaşarsanız backend loglarını kontrol edin:
```
c:\Users\ozkar\OneDrive\Masaüstü\case4\logs\
```
