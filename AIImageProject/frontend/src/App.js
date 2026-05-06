import React, { useState, useRef, useCallback } from "react";
import axios from "axios";
import "./App.css";

function App() {
  const [image, setImage] = useState(null);
  const [preview, setPreview] = useState(null);
  const [result, setResult] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [dragOver, setDragOver] = useState(false);
  const [style, setStyle] = useState("default");
  const fileInputRef = useRef(null);

  const STYLE_PRESETS = [
    { id: "default",   label: "Original",   emoji: "✂️",  desc: "Classic analysis" },
    { id: "anime",     label: "Anime",      emoji: "🏌",  desc: "Japanese anime style" },
    { id: "pixar",     label: "Pixar",      emoji: "🎬",  desc: "3D animated look" },
    { id: "linkedin",  label: "LinkedIn",   emoji: "💼",  desc: "Professional headshot" },
    { id: "cyberpunk", label: "Cyberpunk",  emoji: "🌃",  desc: "Futuristic neon" },
    { id: "watercolor",label: "Watercolor", emoji: "🎨",  desc: "Artistic painting" },
    { id: "retro",     label: "Retro",      emoji: "📼",  desc: "80s vintage vibe" },
  ];

  const handleFile = useCallback((file) => {
    if (!file || !file.type.startsWith("image/")) return;
    setImage(file);
    setPreview(URL.createObjectURL(file));
    setResult("");
    setError("");
  }, []);

  const handleDrop = useCallback((e) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    handleFile(file);
  }, [handleFile]);

  const handleDragOver = useCallback((e) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback(() => {
    setDragOver(false);
  }, []);

  const generateImage = async () => {
    if (!image) return;
    setLoading(true);
    setError("");
    setResult("");

    const formData = new FormData();
    formData.append("image", image);
    formData.append("style", style);

    try {
      const response = await axios.post(
        "http://localhost:5187/api/image/generate",
        formData
      );
      const imageData = response.data.data[0];
      const imageUrl = imageData.url
        || `data:image/jpeg;base64,${imageData.b64_json}`;
      setResult(imageUrl);
    } catch (err) {
      setError(
        err.response?.data || err.message || "Failed to generate image"
      );
    } finally {
      setLoading(false);
    }
  };

  const reset = () => {
    setImage(null);
    setPreview(null);
    setResult("");
    setError("");
    setStyle("default");
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  return (
    <div className="app">
      <div className="bg-blur bg-blur-1" />
      <div className="bg-blur bg-blur-2" />
      <div className="bg-blur bg-blur-3" />

      <div className="bg-emojis" aria-hidden="true">
        <span className="emoji e1">&#128135;</span>
        <span className="emoji e2">&#128131;</span>
        <span className="emoji e3">&#9986;&#65039;</span>
        <span className="emoji e4">&#128133;</span>
        <span className="emoji e5">&#128571;</span>
        <span className="emoji e6">&#10024;</span>
        <span className="emoji e7">&#128134;</span>
        <span className="emoji e8">&#128136;</span>
        <span className="emoji e9">&#128130;</span>
        <span className="emoji e10">&#128132;</span>
        <span className="emoji e11">&#129490;</span>
        <span className="emoji e12">&#128170;</span>
      </div>

      <header className="header">
        <div className="logo">
          <span className="logo-icon">&#9986;</span>
          <span>HairStyle AI</span>
        </div>
        <p className="tagline">Discover your perfect hairstyle with AI</p>
      </header>

      <main className="main">
        {!result ? (
          <div className="upload-section">
            <div
              className={`drop-zone ${dragOver ? "drag-over" : ""} ${preview ? "has-preview" : ""}`}
              onDrop={handleDrop}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={(e) => handleFile(e.target.files[0])}
                hidden
              />

              {preview ? (
                <div className="preview-container">
                  <img src={preview} alt="Preview" className="preview-image" />
                  <div className="preview-overlay">
                    <span>Click or drop to change</span>
                  </div>
                </div>
              ) : (
                <div className="drop-content">
                  <div className="drop-icon">
                    <svg width="48" height="48" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                        d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <p className="drop-title">Drop your portrait here</p>
                  <p className="drop-subtitle">or click to browse</p>
                  <span className="drop-formats">PNG, JPG, JPEG, WebP</span>
                </div>
              )}
            </div>

            {image && (
              <>
                <div className="style-presets">
                  <p className="style-presets-title">Choose a Style</p>
                  <div className="style-grid">
                    {STYLE_PRESETS.map((preset) => (
                      <button
                        key={preset.id}
                        className={`style-btn ${style === preset.id ? "style-active" : ""}`}
                        onClick={() => setStyle(preset.id)}
                        disabled={loading}
                      >
                        <span className="style-emoji">{preset.emoji}</span>
                        <span className="style-label">{preset.label}</span>
                        <span className="style-desc">{preset.desc}</span>
                      </button>
                    ))}
                  </div>
                </div>

                <div className="actions">
                  <button
                    className="btn btn-primary"
                    onClick={generateImage}
                    disabled={loading}
                  >
                    {loading ? (
                      <>
                        <span className="spinner" />
                        <span>Analyzing...</span>
                      </>
                    ) : (
                      <>
                        <span className="btn-icon">&#10024;</span>
                        <span>Analyze as {STYLE_PRESETS.find(p => p.id === style)?.label}</span>
                      </>
                    )}
                  </button>
                  <button className="btn btn-ghost" onClick={reset}>
                    Clear
                  </button>
                </div>
              </>
            )}

            {error && (
              <div className="error-card">
                <span className="error-icon">&#9888;</span>
                <pre>{typeof error === "string" ? error : JSON.stringify(error, null, 2)}</pre>
              </div>
            )}

            {loading && (
              <div className="shimmer-section fade-in">
                <div className="shimmer-card">
                  <div className="shimmer-image" />
                </div>
                <p className="shimmer-text">AI is crafting your hairstyle analysis&hellip;</p>
                <div className="shimmer-progress">
                  <div className="shimmer-progress-bar" />
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="result-section fade-in">
            <div className="result-card">
              <img src={result} alt="Hairstyle analysis" className="result-image" />
            </div>
            <div className="result-actions">
              <a
                href={result}
                download="hairstyle-analysis.jpg"
                className="btn btn-primary"
              >
                <span className="btn-icon">&#8615;</span>
                <span>Download</span>
              </a>
              <button className="btn btn-secondary" onClick={reset}>
                <span className="btn-icon">&#8634;</span>
                <span>Try Another Photo</span>
              </button>
            </div>
          </div>
        )}
      </main>

      {!result && !loading && (
        <section className="info-section">
          <div className="steps">
            <div className="step">
              <div className="step-num">1</div>
              <h3>Upload Photo</h3>
              <p>Drop a portrait or selfie</p>
            </div>
            <div className="step-arrow">&#8594;</div>
            <div className="step">
              <div className="step-num">2</div>
              <h3>AI Analysis</h3>
              <p>GPT-image-1 processes your face</p>
            </div>
            <div className="step-arrow">&#8594;</div>
            <div className="step">
              <div className="step-num">3</div>
              <h3>Get Results</h3>
              <p>See 24+ hairstyle suggestions</p>
            </div>
          </div>

          <div className="categories">
            <h3 className="categories-title">Hairstyles We Analyze</h3>
            <div className="tags">
              {[
                "Buzz Cut", "Crew Cut", "Textured Crop", "Ivy League",
                "Quiff", "Pompadour", "Side Part", "Bro Flow",
                "Man Bun", "Long Waves", "Half Bun",
                "Low Fade", "Mid Fade", "High Fade", "Undercut",
                "Curly Fade", "Afro", "Taper Fade",
                "Modern Mullet", "Wolf Cut", "Two-Block Cut", "Spiky Hair"
              ].map((style) => (
                <span key={style} className="tag">{style}</span>
              ))}
            </div>
          </div>

          <div className="stats">
            <div className="stat">
              <span className="stat-number">24+</span>
              <span className="stat-label">Hairstyles</span>
            </div>
            <div className="stat">
              <span className="stat-number">6</span>
              <span className="stat-label">Categories</span>
            </div>
            <div className="stat">
              <span className="stat-number">AI</span>
              <span className="stat-label">Powered</span>
            </div>
          </div>
        </section>
      )}

      <footer className="footer">
        <p>Powered by Azure OpenAI &middot; GPT-image-1</p>
      </footer>
    </div>
  );
}

export default App;
