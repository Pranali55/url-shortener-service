# 🔗 URL Shortener Service

A production-ready URL shortener built with **.NET 8**, **Python FastAPI**, **Redis**, and **SQL Server** — like bit.ly but self-hosted and fully Dockerized.

## ✨ Features

- 🔗 Shorten any URL with a random 6-char code
- ✏️ Custom aliases (e.g. `/r/my-brand`)
- ⏰ Expiry dates for links
- 📊 Click analytics — daily, weekly, monthly
- 📱 Device & browser detection (Desktop/Mobile/Tablet)
- 🌐 Referer tracking
- ⚡ Redis caching for ultra-fast redirects
- 📈 Python analytics service with smart insights
- 🗂️ Dashboard — total URLs, clicks, active links
- 🔐 JWT Authentication
- 🐳 Fully Dockerized

## 🏗️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 8 (ASP.NET Core) |
| Analytics Service | Python 3.11 + FastAPI |
| Database | SQL Server 2022 |
| Cache | Redis 7 |
| Auth | JWT Bearer Tokens |
| Container | Docker + Docker Compose |

## 🚀 Getting Started

```bash
docker-compose up --build
```

- Swagger UI → [http://localhost:5000/swagger](http://localhost:5000/swagger)
- Analytics API → [http://localhost:8000/docs](http://localhost:8000/docs)

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register |
| POST | `/api/auth/login` | Login |

### URLs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/urls/dashboard` | Dashboard stats |
| GET | `/api/urls` | List my URLs (paginated) |
| POST | `/api/urls` | Create short URL |
| GET | `/api/urls/{id}` | Get URL details |
| GET | `/api/urls/{id}/analytics` | Click analytics |
| PUT | `/api/urls/{id}` | Update URL |
| DELETE | `/api/urls/{id}` | Delete URL |

### Redirect
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/r/{code}` | Redirect to original URL |

## 💡 Example

**Create a short URL:**
```json
POST /api/urls
{
  "originalUrl": "https://www.google.com/very/long/url/here",
  "customAlias": "google",
  "title": "Google Homepage",
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

**Response:**
```json
{
  "shortUrl": "http://localhost:5000/r/google",
  "shortCode": "google",
  "totalClicks": 0
}
```

**Visit:** `http://localhost:5000/r/google` → redirects to Google!

## 📁 Project Structure

```
url-shortener/
├── backend-dotnet/       # .NET 8 API
│   ├── Controllers/      # Auth, Urls, Redirect
│   ├── Services/         # AuthService, UrlService
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   └── Dockerfile
├── python-analytics/     # FastAPI analytics
│   ├── routers/
│   ├── models/
│   ├── services/
│   └── Dockerfile
└── docker-compose.yml
```

## 👨‍💻 Author

Built as a freelance portfolio project.
