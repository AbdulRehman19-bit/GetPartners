# GetPartners

A full-stack matrimonial matchmaking web application built with ASP.NET Core, Entity Framework Core, SignalR, and Supabase (PostgreSQL).

## Features

- JWT-based authentication with BCrypt password hashing
- Profile creation with photo upload
- Browse and filter potential matches by age, city, religion
- Like / pass system with mutual match detection
- Real-time chat between matched users via SignalR

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# |
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (Supabase) |
| Auth | JWT Bearer + BCrypt |
| Real-time | SignalR |
| Frontend | HTML5, CSS3, JavaScript, jQuery, Ajax |
| Testing | Postman |
| Version Control | Git + GitHub |

## Project Structure


## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL database (Supabase free tier recommended)



## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | No | Register new user |
| POST | /api/auth/login | No | Login and get JWT |
| GET | /api/profile | Yes | Get your profile |
| POST | /api/profile | Yes | Create profile |
| PUT | /api/profile | Yes | Update profile |
| POST | /api/profile/photo | Yes | Upload photo |
| GET | /api/browse | Yes | Browse profiles |
| POST | /api/match/like | Yes | Like a profile |
| POST | /api/match/pass | Yes | Pass a profile |
| GET | /api/match/list | Yes | Get your matches |
| GET | /api/messages/{matchId} | Yes | Get chat history |

## License

MIT
