# Hairstyle Analysis - AI Image Generator

An AI-powered hairstyle analysis tool that uses **Azure OpenAI GPT-image-1** to generate hairstyle recommendations from a portrait photo.

## Architecture

```
React Frontend → ASP.NET Core Backend → Azure OpenAI (GPT-image-1) → Generated Image
```

## Features

- Upload a portrait photo
- AI analyzes and generates hairstyle comparisons
- Shows side-by-side hairstyle suggestions with labels
- Covers 24+ hairstyle categories:
  - **Short**: Buzz Cut, Crew Cut, Textured Crop, Ivy League
  - **Medium**: Quiff, Pompadour, Side Part, Bro Flow
  - **Long**: Man Bun, Shoulder-Length, Long Waves, Half Bun
  - **Fade & Undercut**: Low Fade, Mid Fade, High Fade, Undercut
  - **Curly & Textured**: Curly Fade, Afro, Curly Fringe, Taper Fade
  - **Trendy / Modern**: Modern Mullet, Two-Block Cut, Wolf Cut, Spiky Hair

## Tech Stack

| Component | Technology |
|-----------|------------|
| Frontend | React |
| Backend | ASP.NET Core (.NET 9) |
| AI Model | Azure OpenAI GPT-image-1 |
| API Style | REST (multipart/form-data) |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- Azure OpenAI resource with **GPT-image-1** model deployed

## Setup

### 1. Clone the repo

```bash
git clone https://github.com/NagaSanthoshMalipeddy/HairStylesGenerator.git
cd HairStylesGenerator
```

### 2. Configure Azure OpenAI

Edit `AIImageProject/Backend/appsettings.json` and fill in your credentials:

```json
"AzureOpenAI": {
  "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "ApiKey": "YOUR-API-KEY",
  "Deployment": "YOUR-DEPLOYMENT-NAME"
}
```

### 3. Run the Backend

```bash
cd AIImageProject/Backend
dotnet run
```

Backend starts at `http://localhost:5187`.

### 4. Run the Frontend

```bash
cd AIImageProject/frontend
npm install
npm start
```

Frontend opens at `http://localhost:3000`.

### 5. Test

1. Open http://localhost:3000
2. Upload a portrait photo
3. Click **Analyze Hairstyles**
4. View the AI-generated hairstyle analysis

## Project Structure

```
AIImageProject/
├── Backend/
│   ├── Controllers/
│   │   └── ImageController.cs    # API endpoint - sends image to Azure OpenAI
│   ├── Program.cs                # App config, CORS, routing
│   ├── appsettings.json          # Azure OpenAI configuration
│   └── Backend.csproj
├── frontend/
│   ├── src/
│   │   └── App.js                # React UI - upload & display
│   └── package.json
└── AIImageProject.sln
```

## API Reference

### POST `/api/image/generate`

Accepts a portrait image via multipart/form-data and returns an AI-generated hairstyle analysis.

**Request**: `multipart/form-data` with `image` field

**Response**: JSON with base64-encoded image in `data[0].b64_json`

## License

MIT
