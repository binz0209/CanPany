# 🚀 Hướng dẫn Setup Environment - CanPany

Hướng dẫn chi tiết để cài đặt và cấu hình môi trường phát triển cho dự án CanPany.

## 📋 Mục lục

- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt Dependencies](#cài-đặt-dependencies)
- [Setup MongoDB](#setup-mongodb)
- [Setup Redis](#setup-redis)
- [Setup Cloudinary](#setup-cloudinary)
- [Setup VNPay](#setup-vnpay)
- [Setup Email Service](#setup-email-service)
- [Setup Google OAuth](#setup-google-oauth)
- [Setup Gemini API](#setup-gemini-api)
- [Cấu hình Backend](#cấu-hình-backend)
- [Cấu hình Frontend](#cấu-hình-frontend)
- [Chạy ứng dụng](#chạy-ứng-dụng)
- [Troubleshooting](#troubleshooting)

---

## 💻 Yêu cầu hệ thống

### Bắt buộc
- **Node.js** >= 18.x
- **.NET SDK** >= 8.0
- **MongoDB** >= 6.0 (hoặc MongoDB Atlas account)
- **Git** >= 2.0

### Tùy chọn (cho Background Jobs)
- **Redis** >= 6.0 (hoặc Redis Cloud account)

### IDE/Editor
- **Visual Studio 2022** hoặc **VS Code** (khuyến nghị)
- **Postman** hoặc **Thunder Client** (để test API)

---

## 📦 Cài đặt Dependencies

### 1. Node.js

#### Windows
1. Tải Node.js từ [nodejs.org](https://nodejs.org/)
2. Chọn phiên bản LTS (>= 18.x)
3. Chạy installer và làm theo hướng dẫn
4. Verify installation:
```bash
node --version
npm --version
```

#### macOS
```bash
# Sử dụng Homebrew
brew install node@18

# Hoặc tải từ nodejs.org
```

#### Linux (Ubuntu/Debian)
```bash
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

### 2. .NET SDK 8.0

#### Windows
1. Tải .NET SDK từ [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Chạy installer
3. Verify installation:
```bash
dotnet --version
```

#### macOS
```bash
brew install dotnet@8
```

#### Linux (Ubuntu/Debian)
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version 8.0.0
```

### 3. MongoDB

#### Option A: MongoDB Atlas (Khuyến nghị cho Development)

1. Đăng ký tài khoản tại [mongodb.com/cloud/atlas](https://www.mongodb.com/cloud/atlas)
2. Tạo cluster miễn phí (M0 - Free Tier)
3. Tạo database user:
   - Username: `your-username`
   - Password: `your-password`
4. Whitelist IP: Thêm `0.0.0.0/0` để cho phép kết nối từ mọi nơi (chỉ cho dev)
5. Lấy Connection String:
   - Click "Connect" → "Connect your application"
   - Copy connection string dạng: `mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority`

#### Option B: MongoDB Local

##### Windows
1. Tải MongoDB Community Server từ [mongodb.com/try/download/community](https://www.mongodb.com/try/download/community)
2. Chạy installer
3. MongoDB sẽ chạy như Windows Service
4. Connection String: `mongodb://localhost:27017`

##### macOS
```bash
brew tap mongodb/brew
brew install mongodb-community@6.0
brew services start mongodb-community@6.0
```

##### Linux (Ubuntu/Debian)
```bash
wget -qO - https://www.mongodb.org/static/pgp/server-6.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/6.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-6.0.list
sudo apt-get update
sudo apt-get install -y mongodb-org
sudo systemctl start mongod
sudo systemctl enable mongod
```

### 4. Redis (Tùy chọn - cho Background Jobs)

#### Option A: Redis Cloud (Khuyến nghị)

1. Đăng ký tại [redis.com/try-free](https://redis.com/try-free/)
2. Tạo database miễn phí
3. Lấy connection string: `redis://default:password@host:port`

#### Option B: Redis Local

##### Windows
1. Tải Redis từ [github.com/microsoftarchive/redis/releases](https://github.com/microsoftarchive/redis/releases)
2. Hoặc sử dụng WSL2 với Redis
3. Connection String: `localhost:6379`

##### macOS
```bash
brew install redis
brew services start redis
```

##### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install redis-server
sudo systemctl start redis-server
sudo systemctl enable redis-server
```

Verify Redis:
```bash
redis-cli ping
# Should return: PONG
```

---

## ☁️ Setup Cloudinary

Cloudinary được sử dụng để upload và quản lý hình ảnh.

1. Đăng ký tài khoản tại [cloudinary.com](https://cloudinary.com/)
2. Vào Dashboard → Settings
3. Copy các thông tin sau:
   - **Cloud Name**: `your-cloud-name` (ví dụ: `abc123xyz`)
   - **API Key**: `your-api-key` (ví dụ: `123456789012345`)
   - **API Secret**: `your-api-secret` (ví dụ: `AbCdEfGhIjKlMnOpQrStUvWxYz`)

**Lưu ý**: Thông tin trên là example, bạn cần tạo tài khoản riêng và sử dụng credentials của mình.

---

## 💳 Setup VNPay

VNPay được sử dụng để xử lý thanh toán.

1. Đăng ký tài khoản tại [vnpay.vn](https://vnpay.vn/)
2. Tạo merchant account
3. Lấy thông tin:
   - **TmnCode**: Mã merchant
   - **HashSecret**: Secret key
   - **PaymentUrl**: 
     - Sandbox: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
     - Production: `https://vnpayment.vn/paymentv2/vpcpay.html`

**Lưu ý**: Để test, bạn có thể sử dụng sandbox credentials hoặc bỏ qua phần này nếu không cần test payment.

---

## 📧 Setup Email Service

Ứng dụng sử dụng Gmail SMTP để gửi email.

### Tạo Gmail App Password

1. Vào [Google Account](https://myaccount.google.com/)
2. Security → 2-Step Verification (bật nếu chưa bật)
3. App passwords → Generate app password
4. Chọn "Mail" và "Other (Custom name)"
5. Đặt tên: "CanPany"
6. Copy password (16 ký tự, không có khoảng trắng)

### Cấu hình Email

- **SmtpHost**: `smtp.gmail.com`
- **SmtpPort**: `587`
- **FromEmail**: Email Gmail của bạn
- **FromName**: `CanPany` (hoặc tên bạn muốn)
- **Password**: App password vừa tạo

---

## 🔐 Setup Google OAuth

Để đăng nhập bằng Google:

1. Vào [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project có sẵn
3. Enable Google+ API:
   - APIs & Services → Library
   - Tìm "Google+ API" → Enable
4. Tạo OAuth 2.0 Credentials:
   - APIs & Services → Credentials
   - Create Credentials → OAuth client ID
   - Application type: Web application
   - Authorized redirect URIs:
     - `http://localhost:5174` (development)
     - `https://yourdomain.com` (production)
5. Copy **Client ID**

---

## 🤖 Setup Gemini API (Tùy chọn)

Gemini API được sử dụng cho AI features (CV analysis, project recommendations).

1. Vào [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Tạo API key mới
3. Copy API key

**Lưu ý**: Tính năng này là tùy chọn, bạn có thể bỏ qua nếu không cần AI features.

---

## ⚙️ Cấu hình Backend

### 1. Tạo file `appsettings.json`

Tạo file `CanPany-BE/CanPany.Api/appsettings.json`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority",
    "DbName": "CanPanyDev",
    "Collections": {
      "Users": "users",
      "UserProfiles": "user_profiles",
      "Categories": "categories",
      "Skills": "skills",
      "Projects": "projects",
      "ProjectSkills": "project_skills",
      "Proposals": "proposals",
      "Contracts": "contracts",
      "Payments": "payments",
      "Messages": "messages",
      "Notifications": "notifications",
      "Reviews": "reviews",
      "Wallets": "wallets",
      "WalletTransactions": "wallet_transactions",
      "UserSettings": "user_settings",
      "Banners": "banners",
      "Companies": "companies",
      "Jobs": "jobs",
      "CVs": "cvs",
      "CVAnalyses": "cv_analyses",
      "Applications": "applications",
      "PremiumPackages": "premium_packages",
      "Reports": "reports",
      "ExternalSyncs": "external_syncs",
      "AuditLogs": "audit_logs"
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:5174"
    ]
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long-for-security"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Vnpay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "ReturnUrl": "http://localhost:5174/payment-success",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
  },
  "TimeZoneId": "SE Asia Standard Time",
  "Frontend": {
    "BaseUrl": "http://localhost:5174"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your-email@gmail.com",
    "FromName": "CanPany",
    "Password": "your-app-password"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "BackgroundJobs": {
    "WorkerCount": 2,
    "PollInterval": 1000
  }
}
```

### 2. Cấu hình JWT Key

Tạo JWT secret key (tối thiểu 32 ký tự):

```bash
# Windows PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})

# Linux/macOS
openssl rand -base64 32
```

Thay thế `"Key"` trong `Jwt` section bằng key vừa tạo.

### 3. Restore NuGet Packages

```bash
cd CanPany-BE
dotnet restore
```

### 4. Verify Backend Configuration

```bash
cd CanPany-BE/CanPany.Api
dotnet build
```

Nếu build thành công, bạn đã cấu hình đúng!

---

## 🎨 Cấu hình Frontend

### 1. Tạo file `.env`

Tạo file `CanPany-FE/.env`:

```env
# API Base URL
VITE_API_URL=http://localhost:5070/api

# Google OAuth Client ID
VITE_GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
```

### 2. Cài đặt Dependencies

```bash
cd CanPany-FE
npm install
```

### 3. Verify Frontend Configuration

```bash
npm run build
```

Nếu build thành công, bạn đã cấu hình đúng!

---

## 🚀 Chạy ứng dụng

### 1. Start MongoDB (nếu dùng local)

```bash
# Windows (nếu cài như service, tự động start)
# Hoặc:
mongod --dbpath "C:\data\db"

# macOS
brew services start mongodb-community@6.0

# Linux
sudo systemctl start mongod
```

### 2. Start Redis (nếu dùng local và cần Background Jobs)

```bash
# Windows
redis-server

# macOS
brew services start redis

# Linux
sudo systemctl start redis-server
```

### 3. Start Backend

```bash
cd CanPany-BE/CanPany.Api
dotnet run
```

Backend sẽ chạy tại: `http://localhost:5070`
Swagger UI: `http://localhost:5070/swagger`

### 4. Start Frontend

```bash
cd CanPany-FE
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:5174`

### 5. Verify Setup

1. Mở browser: `http://localhost:5174`
2. Kiểm tra console không có lỗi
3. Thử đăng ký tài khoản mới
4. Kiểm tra MongoDB có tạo collections không

---

## 🔧 Troubleshooting

### Lỗi MongoDB Connection

**Lỗi**: `MongoDB connection failed`

**Giải pháp**:
1. Kiểm tra MongoDB đang chạy:
   ```bash
   # Windows
   net start MongoDB
   
   # macOS/Linux
   sudo systemctl status mongod
   ```
2. Kiểm tra Connection String trong `appsettings.json`
3. Nếu dùng Atlas, kiểm tra IP whitelist
4. Kiểm tra username/password đúng

### Lỗi Redis Connection

**Lỗi**: `Redis connection failed`

**Giải pháp**:
1. Redis là tùy chọn, nếu không cần Background Jobs, có thể bỏ qua
2. Nếu cần, kiểm tra Redis đang chạy:
   ```bash
   redis-cli ping
   ```
3. Kiểm tra connection string trong `appsettings.json`

### Lỗi CORS

**Lỗi**: `CORS policy blocked`

**Giải pháp**:
1. Kiểm tra `Cors:AllowedOrigins` trong `appsettings.json`
2. Đảm bảo frontend URL đúng (http://localhost:5174)
3. Restart backend sau khi sửa CORS config

### Lỗi JWT Authentication

**Lỗi**: `Invalid token` hoặc `Unauthorized`

**Giải pháp**:
1. Kiểm tra JWT Key trong `appsettings.json` (tối thiểu 32 ký tự)
2. Clear localStorage trong browser:
   ```javascript
   localStorage.clear()
   ```
3. Đăng nhập lại

### Lỗi Cloudinary Upload

**Lỗi**: `Cloudinary upload failed`

**Giải pháp**:
1. Kiểm tra Cloudinary credentials trong `appsettings.json`
2. Verify Cloudinary account đang active
3. Kiểm tra quota/limits

### Lỗi Email Sending

**Lỗi**: `Email sending failed`

**Giải pháp**:
1. Kiểm tra Gmail App Password đúng
2. Đảm bảo 2-Step Verification đã bật
3. Kiểm tra SMTP settings trong `appsettings.json`

### Lỗi Frontend Build

**Lỗi**: `Build failed` hoặc `Module not found`

**Giải pháp**:
```bash
cd CanPany-FE
rm -rf node_modules package-lock.json
npm install
npm run build
```

### Lỗi Backend Build

**Lỗi**: `Build failed` hoặc `Package restore failed`

**Giải pháp**:
```bash
cd CanPany-BE
dotnet clean
dotnet restore
dotnet build
```

### Port đã được sử dụng

**Lỗi**: `Port 5070 already in use` hoặc `Port 5174 already in use`

**Giải pháp**:

**Backend**:
- Sửa port trong `launchSettings.json`:
```json
"applicationUrl": "http://localhost:5071"
```

**Frontend**:
- Sửa port trong `vite.config.js`:
```javascript
server: {
  port: 5175,
}
```

Hoặc kill process đang dùng port:
```bash
# Windows
netstat -ano | findstr :5070
taskkill /PID <PID> /F

# macOS/Linux
lsof -ti:5070 | xargs kill -9
```

---

## 📝 Checklist Setup

Sử dụng checklist này để đảm bảo bạn đã setup đầy đủ:

### Dependencies
- [ ] Node.js >= 18.x đã cài
- [ ] .NET SDK 8.0 đã cài
- [ ] MongoDB đã setup (local hoặc Atlas)
- [ ] Redis đã setup (nếu cần Background Jobs)

### External Services
- [ ] Cloudinary account đã tạo và có credentials
- [ ] VNPay account đã tạo (hoặc bỏ qua nếu không test payment)
- [ ] Gmail App Password đã tạo
- [ ] Google OAuth Client ID đã tạo
- [ ] Gemini API Key đã tạo (tùy chọn)

### Configuration
- [ ] `appsettings.json` đã được cấu hình đầy đủ
- [ ] JWT Key đã được tạo (>= 32 ký tự)
- [ ] `.env` file đã được tạo trong Frontend
- [ ] MongoDB Connection String đúng
- [ ] CORS origins đã được cấu hình

### Verification
- [ ] Backend build thành công
- [ ] Frontend build thành công
- [ ] Backend chạy được tại http://localhost:5070
- [ ] Frontend chạy được tại http://localhost:5174
- [ ] Swagger UI accessible tại http://localhost:5070/swagger
- [ ] Có thể đăng ký tài khoản mới
- [ ] MongoDB collections được tạo tự động

---

## 🎯 Quick Start (Tóm tắt)

```bash
# 1. Clone repository
git clone <repository-url>
cd CanPany

# 2. Setup Backend
cd CanPany-BE/CanPany.Api
# Tạo appsettings.json với config trên
dotnet restore
dotnet build

# 3. Setup Frontend
cd ../../CanPany-FE
# Tạo .env với config trên
npm install

# 4. Start MongoDB (nếu local)
# Windows: mongod
# macOS: brew services start mongodb-community@6.0
# Linux: sudo systemctl start mongod

# 5. Start Backend
cd ../CanPany-BE/CanPany.Api
dotnet run

# 6. Start Frontend (terminal mới)
cd ../../CanPany-FE
npm run dev

# 7. Mở browser: http://localhost:5174
```

---

## 📚 Tài liệu tham khảo

- [Node.js Documentation](https://nodejs.org/docs/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Redis Documentation](https://redis.io/docs/)
- [Cloudinary Documentation](https://cloudinary.com/documentation)
- [VNPay Documentation](https://sandbox.vnpayment.vn/apis/)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)

---

## 💡 Tips

1. **Development**: Sử dụng MongoDB Atlas và Redis Cloud để không cần cài local
2. **Security**: Không commit `appsettings.json` và `.env` vào Git (đã có trong .gitignore)
3. **Debugging**: Sử dụng Swagger UI để test API endpoints
4. **Logs**: Kiểm tra console logs của Backend để debug
5. **Hot Reload**: Frontend tự động reload khi code thay đổi, Backend cần restart

---

**Chúc bạn setup thành công! 🎉**

Nếu gặp vấn đề, hãy kiểm tra phần [Troubleshooting](#troubleshooting) hoặc tạo issue trên repository.

