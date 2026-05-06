import React, { useState } from "react";
import axios from "axios";

function App() {
  const [image, setImage] = useState(null);
  const [result, setResult] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const generateImage = async () => {
    if (!image) return;

    setLoading(true);
    setError("");
    setResult("");

    const formData = new FormData();
    formData.append("image", image);

    try {
      const response = await axios.post(
        "http://localhost:5187/api/image/generate",
        formData
      );

      const imageData = response.data.data[0];
      // Handle both b64_json and url response formats
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

  return (
    <div style={{ padding: "40px", maxWidth: "800px", margin: "0 auto" }}>
      <h1>Hairstyle Analysis</h1>
      <p>Upload a portrait photo to see which hairstyles suit you best.</p>

      <input
        type="file"
        accept="image/*"
        onChange={(e) => setImage(e.target.files[0])}
      />

      <br /><br />

      <button
        onClick={generateImage}
        disabled={loading || !image}
        style={{ padding: "10px 24px", fontSize: "16px" }}
      >
        {loading ? "Generating..." : "Analyze Hairstyles"}
      </button>

      <br /><br />

      {error && (
        <div style={{ color: "red", whiteSpace: "pre-wrap" }}>
          {typeof error === "string" ? error : JSON.stringify(error, null, 2)}
        </div>
      )}

      {result && (
        <div>
          <img src={result} alt="hairstyle analysis" width="100%" />
          <br />
          <a href={result} target="_blank" rel="noopener noreferrer">
            Open full-size image
          </a>
        </div>
      )}
    </div>
  );
}

export default App;
