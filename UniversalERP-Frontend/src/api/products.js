import { httpGet, httpPost, httpPut, httpPatch, httpDelete } from "./http";

// Sistemdeki tüm ürünlerin listesini getiren asenkron fonksiyon
export async function getProducts() {
  return httpGet("/api/products");
}

// Kritik stok seviyesinin (minStockLevel) altına düşen ürünleri listeleyen fonksiyon
export async function getLowStockProducts() {
  return httpGet("/api/products/low-stock");
}

// Benzersiz kimlik (ID) değerine göre tek bir ürünün detayını getiren fonksiyon
export async function getProduct(id) {
  return httpGet(`/api/products/${id}`);
}

// Yeni bir ürün oluşturan fonksiyon
// Gönderilecek Nesne (Body): { name, price, stockCount, minStockLevel }
export async function createProduct(dto) {
  return httpPost("/api/products", dto);
}

// Mevcut bir ürünün temel bilgilerini bütünüyle güncelleyen fonksiyon (PUT)
// Gönderilecek Nesne (Body): { name, price, minStockLevel, isActive }
export async function updateProduct(id, dto) {
  return httpPut(`/api/products/${id}`, dto);
}

// Ürünün sadece stok miktarını güncelleyen kısmi güncelleme fonksiyonu (PATCH)
// Gönderilecek Nesne (Body): { newStock }
export async function updateProductStock(id, newStock) {
  return httpPatch(`/api/products/${id}/stock`, { newStock });
}

// Belirtilen kimlik (ID) değerine sahip ürünü sistemden silen fonksiyon
export async function deleteProduct(id) {
  return httpDelete(`/api/products/${id}`);
}