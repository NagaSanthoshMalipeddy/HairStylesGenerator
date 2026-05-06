# ✂️ HairStyle AI — AI-Powered Hairstyle Analysis

Upload a portrait photo and let **Azure OpenAI GPT-image-1** generate a visual hairstyle analysis showing 24+ hairstyle suggestions that suit your face.

![Tech Stack](https://img.shields.io/badge/.NET_9-512BD4?style=flat&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?style=flat&logo=react&logoColor=black)
![Azure](https://img.shields.io/badge/Azure_OpenAI-0078D4?style=flat&logo=microsoftazure&logoColor=white)

---

## 📐 Architecture

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│  React Frontend  │────▶│  ASP.NET Core API    │────▶│  Azure OpenAI        │
│  (localhost:3000)│◀────│  (localhost:5187)     │◀────│  GPT-image-1         │
└─────────────────┘     └──────────────────────┘     └─────────────────────┘
        Upload                 Proxy + Auth               Image Editing
        + Display              multipart/form-data         (base64 response)
```

**Flow:**
1. User uploads a portrait image on the React frontend
2. Frontend sends the image to the .NET backend via `POST /api/image/generate`
3. Backend forwards the image + a hardcoded prompt to Azure OpenAI `/images/edits` endpoint
4. Azure OpenAI (GPT-image-1) generates a hairstyle analysis graphic
5. Backend returns the base64-encoded result to the frontend
6. Frontend renders the generated image

---

## ✨ Features

- 🖼️ **Drag & drop** or click to upload portrait photos
- 🔍 **Image preview** before generating
- ✨ **Shimmer loading** animation during AI processing
- 📱 **Fully responsive** — works on desktop, tablet, and mobile
- 🌙 **Dark glassmorphism UI** with animated gradient background
- 💾 **Download** generated analysis as image
- 🔄 **Try another photo** — easy reset flow
- 🏷️ **24+ hairstyle** suggestions across 6 categories

### Hairstyle Categories

| Category | Styles |
|----------|--------|
| **Short** | Buzz Cut, Crew Cut, Textured Crop, Ivy League |
| **Medium-Length** | Quiff, Pompadour, Side Part, Bro Flow |
| **Long** | Man Bun, Shoulder-Length Hair, Long Waves, Half Bun |
| **Fade & Undercut** | Low Fade, Mid Fade, High Fade, Undercut |
| **Curly & Textured** | Curly Fade, Afro, Curly Fringe, Taper Fade |
| **Trendy / Modern** | Modern Mullet, Two-Block Cut, Wolf Cut, Spiky Hair |

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| Frontend | React 18 | UI — upload, preview, display results |
| Backend | ASP.NET Core (.NET 9) | API proxy — auth, forward to Azure |
| AI Model | Azure OpenAI GPT-image-1 | Image editing / generation |
| Styling | Custom CSS | Glassmorphism, responsive, animations |
| HTTP Client | Axios (frontend), HttpClient (.NET) | API calls |
| Font | Inter (Google Fonts) | Typography |

---

## 📋 Prerequisites

Before you begin, make sure you have:

- [**.NET 9 SDK**](https://dotnet.microsoft.com/download/dotnet/9.0) — to run the backend
- [**Node.js v18+**](https://nodejs.org/) — to run the React frontend
- **Azure OpenAI resource** with a **GPT-image-1** (or compatible image model) deployment
  - You'll need: **Endpoint URL**, **API Key**, and **Deployment Name**
  - Recommended regions: **East US**, **West US 3**, **Sweden Central**

---

## 🚀 Setup & Installation

### Step 1: Clone the Repository

```bash
git clone https://github.com/NagaSanthoshMalipeddy/HairStylesGenerator.git
cd HairStylesGenerator
```

### Step 2: Configure Azure OpenAI Credentials

Open `AIImageProject/Backend/appsettings.json` and replace the placeholders:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE-NAME.openai.azure.com/",
    "ApiKey": "YOUR-AZURE-OPENAI-API-KEY",
    "Deployment": "YOUR-DEPLOYMENT-NAME"
  }
}
```

> ⚠️ **Important:**
> - Endpoint must end with `/`
> - Never commit your API key to version control
> - You can find these values in the [Azure AI Foundry portal](https://ai.azure.com/) under your deployment

### Step 3: Run the Backend

```bash
cd AIImageProject/Backend
dotnet restore
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5187
```

The backend API is now running at **http://localhost:5187**.

### Step 4: Run the Frontend

Open a **new terminal** window:

```bash
cd AIImageProject/frontend
npm install
npm start
```

The React app opens automatically at **http://localhost:3000**.

---

## 🎯 How to Use

1. **Open** http://localhost:3000 in your browser
2. **Upload** a portrait photo:
   - Drag and drop onto the upload area, OR
   - Click the upload area to browse files
   - Supported formats: PNG, JPG, JPEG, WebP
3. **Preview** your uploaded image in the drop zone
4. **Click** "Analyze Hairstyles"
5. **Wait** for the AI to generate (you'll see a shimmer loading animation)
6. **View** your personalized hairstyle analysis graphic
7. **Download** the result or click "Try Another Photo" to start over

---

## 📁 Project Structure

```
HairStylesGenerator/
├── .gitignore
├── README.md
├── AIImageProject/
│   ├── AIImageProject.sln              # Solution file
│   ├── Backend/
│   │   ├── Backend.csproj              # .NET project file
│   │   ├── Program.cs                  # App startup — CORS, routing, controllers
│   │   ├── appsettings.json            # Azure OpenAI config (add your keys here)
│   │   ├── appsettings.Development.json
│   │   ├── Properties/
│   │   │   └── launchSettings.json     # Dev server ports
│   │   └── Controllers/
│   │       └── ImageController.cs      # POST /api/image/generate endpoint
│   └── frontend/
│       ├── package.json
│       ├── public/
│       │   └── index.html              # HTML shell with Inter font
│       └── src/
│           ├── App.js                  # Main React component — UI logic
│           ├── App.css                 # Glassmorphism styles & animations
│           ├── index.js                # React entry point
│           └── index.css               # Global styles & dark theme
```

---

## 🔌 API Reference

### `POST /api/image/generate`

Accepts a portrait image and returns an AI-generated hairstyle analysis.

**Request:**
- Content-Type: `multipart/form-data`
- Body field: `image` (file)

**What the backend does:**
- Reads the uploaded image
- Sends it to Azure OpenAI `/images/edits` endpoint with:
  - `prompt`: Hardcoded hairstyle analysis prompt
  - `n`: 1
  - `size`: 1024x1024
  - `quality`: medium
  - `output_format`: jpeg
  - `output_compression`: 100
- Uses `Authorization: Bearer <api-key>` header
- API version: `2025-04-01-preview`

**Response:**
```json
{
  "data": [
    {
      "b64_json": "<base64-encoded-image>"
    }
  ]
}
```

---

## 🐛 Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| **Network Error** | Backend not running or CORS issue | Ensure backend is running on port 5187. Check `Program.cs` has `app.UseCors("AllowAll")` |
| **404 Not Found** | Wrong deployment name or API version | Verify deployment name in Azure AI Foundry matches `appsettings.json` |
| **401 Unauthorized** | Wrong API key | Double-check `ApiKey` in `appsettings.json` |
| **307 Redirect** | HTTPS redirect enabled | `UseHttpsRedirection()` has been removed from `Program.cs` — make sure it stays removed |
| **413 Payload Too Large** | Image file too big | Try a smaller image (under 4MB recommended) |
| **Region not supported** | Image model not available | Use East US, West US 3, or Sweden Central regions |
| **CORS error in browser** | Frontend can't reach backend | Backend must be on `http://localhost:5187`, frontend on `http://localhost:3000` |

---

## 🔒 Security Notes

- API keys are stored in `appsettings.json` — **never commit real keys to Git**
- The `.gitignore` excludes `bin/`, `obj/`, `node_modules/`, and `.env` files
- For production, use [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/) or environment variables
- CORS is set to allow all origins for development — restrict in production

---

## 🚀 Future Improvements

- [ ] Material UI / Tailwind CSS upgrade
- [ ] Before/after comparison view
- [ ] Image history / gallery
- [ ] Preset style buttons (Anime, Pixar, LinkedIn, Cyberpunk)
- [ ] Deploy to Azure (Static Web App + App Service)
- [ ] User authentication
- [ ] Save favorites

---

## 📄 License

MIT

---

**Built with ❤️ using React, .NET 9, and Azure OpenAI GPT-image-1**
