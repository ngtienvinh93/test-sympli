import React, { useState } from 'react';
import httpService from '../../Services/httpServices';

const SeoResult = () => {
  const [keywords, setKeywords] = useState("e-settlements");
  const [url, setUrl] = useState("https://www.sympli.com.au");
  const [seoResult, setSeoResult] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const fetchSeoResult = async () => {
    setLoading(true);
    setError("");
    try {
      const result = await httpService.getSeoResult(keywords, url);
      setSeoResult(result);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>SEO Result Checker</h1>
      <div style={styles.form}>
        <input
          type="text"
          placeholder="Enter keywords"
          value={keywords}
          onChange={(e) => setKeywords(e.target.value)}
          style={styles.input}
        />
        <input
          type="text"
          placeholder="Enter URL"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          style={styles.input}
        />
        <button onClick={fetchSeoResult} disabled={loading} style={styles.button}>
          {loading ? "Loading..." : "Get SEO Result"}
        </button>
      </div>
      {error && <p style={styles.error}>{error}</p>}
      {seoResult && (
        <div style={styles.resultBox}>
          <h3>SEO Results</h3>
          <p>{seoResult}</p>
        </div>
      )}
    </div>
  );
};

const styles = {
  container: {
    maxWidth: "600px",
    margin: "50px auto",
    padding: "20px",
    fontFamily: "'Arial', sans-serif",
    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)",
    borderRadius: "10px",
    backgroundColor: "#f9f9f9",
  },
  title: {
    textAlign: "center",
    color: "#333",
  },
  form: {
    display: "flex",
    flexDirection: "column",
    gap: "10px",
    marginTop: "20px",
  },
  input: {
    padding: "10px",
    fontSize: "16px",
    borderRadius: "5px",
    border: "1px solid #ccc",
  },
  button: {
    padding: "10px",
    fontSize: "16px",
    color: "#fff",
    backgroundColor: "#007bff",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
  },
  error: {
    color: "red",
    marginTop: "10px",
    textAlign: "center",
  },
  resultBox: {
    marginTop: "20px",
    padding: "15px",
    border: "1px solid #007bff",
    borderRadius: "5px",
    backgroundColor: "#e9f7fe",
  },
};

export default SeoResult;
