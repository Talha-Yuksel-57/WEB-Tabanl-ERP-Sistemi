import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./styles.css";

// Sayfa render olmadan önce kayıtlı temayı uygula, böylece açık temadan
// koyu temaya geçiş anında bir "flaş" oluşmaz.
const savedTheme = localStorage.getItem("erp_theme") || "light";
document.documentElement.setAttribute("data-theme", savedTheme);

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
