import { httpGet, httpPost } from "./http";

// Sistemde gerçekleştirilmiş tüm satışların listesini getiren fonksiyon
export async function getSales() {
  return httpGet("/api/sales");
}

// Belirtilen benzersiz kimlik (ID) değerine sahip tek bir satışın detayını getiren fonksiyon
export async function getSale(id) {
  return httpGet(`/api/sales/${id}`);
}

// Yeni bir satış işlemi gerçekleştiren fonksiyon
// Gönderilecek Nesne (Body): { productId, customerId, quantity, paymentMethod }
// Backend Notu: Bu işlem backend tarafında bütünsel (ACID) bir transaction olarak yürütülür.
// Ürün stok miktarının düşürülmesi ve satış kaydının oluşturulması adımları tek bir işlemde bağlanmıştır;
// Adımlardan biri başarısız olursa (örneğin stok yetersizse), tüm süreç veritabanı düzeyinde geri alınır (Rollback).
export async function createSale(dto) {
  return httpPost("/api/sales", dto);
}

// Gerçekleştirilmiş bir satışı iptal eden fonksiyon
// İptal isteği güvenli bir şekilde POST metoduyla ve boş bir nesne ({}) gönderilerek tetiklenir
export async function cancelSale(id) {
  return httpPost(`/api/sales/${id}/cancel`, {});
}