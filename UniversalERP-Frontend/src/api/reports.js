import { BASE_URL } from "./http";

// Yerel depolamadan (localStorage) kullanıcı yetkilendirme token'ını alan yardımcı fonksiyon
function getToken() {
  return localStorage.getItem("erp_token") || "";
}

// URL parametrelerini (Query String) dinamik ve temiz bir şekilde oluşturan yardımcı fonksiyon
// Tanımsız (undefined), boş ("") veya null olan filtre alanlarını temizleyerek URL'e eklemez
function buildQuery(params) {
  const usable = Object.entries(params || {}).filter(
    ([, v]) => v !== undefined && v !== null && v !== ""
  );
  if (usable.length === 0) return "";
  const qs = new URLSearchParams(usable);
  return `?${qs.toString()}`;
}

// Sunucudan binary veri (Blob) olarak dosya indiren ve tarayıcıda indirme işlemini başlatan ortak fonksiyon
async function downloadFile(path, fallbackName) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "GET",
    headers: { Authorization: `Bearer ${getToken()}` }, // JWT Token backend doğrulaması için eklenir
  });

  // HTTP hata kontrolü ve backend'den dönen hata mesajını yakalama senaryosu
  if (!res.ok) {
    let message = `HTTP ${res.status}`;
    try {
      const data = await res.json();
      message = data.message || message;
    } catch {
      /* response body JSON değilse hatayı doğrudan HTTP koduyla fırlat */
    }
    throw new Error(message);
  }

  // Gelen yanıtı ikili büyük nesneye (Blob - Binary Large Object) dönüştürür
  const blob = await res.blob();

  // Sunucudan gelen Content-Disposition başlığından orijinal dosya adını ayıklar, yoksa varsayılan adı kullanır
  const disposition = res.headers.get("Content-Disposition") || "";
  const match = disposition.match(/filename="?([^"]+)"?/);
  const fileName = match ? match[1] : fallbackName;

  // Sanal bir indirme bağlantısı (DOM elemanı) oluşturarak dosyayı tarayıcıda otomatik indirir
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click(); // İndirmeyi tetikler
  a.remove(); // Elemanı DOM'dan temizler
  window.URL.revokeObjectURL(url); // Bellek sızıntısını önlemek için sanal URL'i serbest bırakır
}

// Filtre parametrelerine göre satış raporunu PDF formatında indiren fonksiyon
export async function downloadSalesPdf(filter) {
  const qs = buildQuery(filter);
  return downloadFile(`/api/reports/sales/pdf${qs}`, "satis-raporu.pdf");
}

// Filtre parametrelerine göre satış raporunu Excel formatında indiren fonksiyon
export async function downloadSalesExcel(filter) {
  const qs = buildQuery(filter);
  return downloadFile(`/api/reports/sales/excel${qs}`, "satis-raporu.xlsx");
}

// Mevcut güncel stok raporunu PDF formatında indiren fonksiyon
export async function downloadStockPdf() {
  return downloadFile("/api/reports/stock/pdf", "stok-raporu.pdf");
}

// Mevcut güncel stok raporunu Excel formatında indiren fonksiyon
export async function downloadStockExcel() {
  return downloadFile("/api/reports/stock/excel", "stok-raporu.xlsx");
}

// --- ÖNİZLEME (JSON) ---

// Rapor verilerini indirmeden önce tabloda veya ekranda listelemek için JSON formatında çeken ortak fonksiyon
async function getJson(path) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  });
  const data = await res.json();
  if (!res.ok) throw new Error(data?.message || `HTTP ${res.status}`);
  return data;
}

// Satış raporu verilerinin ekranda önizlemesini (tablo dizisi olarak) getiren fonksiyon
export async function previewSales(filter) {
  const qs = buildQuery(filter);
  return getJson(`/api/reports/sales/preview${qs}`);
}

// Stok durumu verilerinin ekranda önizlemesini getiren fonksiyon
export async function previewStock() {
  return getJson("/api/reports/stock/preview");
}

// --- IMPORT (multipart/form-data) ---

// Sunucuya dosya yükleme (Upload) işlemlerini yürüten ortak asenkron fonksiyon
async function uploadFile(path, file) {
  const formData = new FormData();
  formData.append("file", file); // Dosya verisini multipart/form-data yapısına hazırlar

  const res = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: { Authorization: `Bearer ${getToken()}` }, // Tarayıcı, boundary içeren Content-Type başlığını otomatik ekler
    body: formData,
  });

  const data = await res.json();
  if (!res.ok) throw new Error(data?.message || `HTTP ${res.status}`);
  return data;
}

// Dışarıdan hazırlanan Excel dosyasındaki ürünleri sisteme toplu aktaran (Import) fonksiyon
export async function importProductsExcel(file) {
  return uploadFile("/api/reports/products/import/excel", file);
}

// Dışarıdan hazırlanan JSON dosyasındaki ürünleri sisteme toplu aktaran fonksiyon
export async function importProductsJson(file) {
  return uploadFile("/api/reports/products/import/json", file);
}

// Dışarıdan hazırlanan XML dosyasındaki ürünleri sisteme toplu aktaran fonksiyon
export async function importProductsXml(file) {
  return uploadFile("/api/reports/products/import/xml", file);
}