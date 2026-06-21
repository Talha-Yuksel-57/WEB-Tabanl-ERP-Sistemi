// HTTP GET isteklerini gerçekleştiren ortak fonksiyon içe aktarılır.
import { httpGet } from "./http";

/**
 * Dashboard ekranında görüntülenecek verileri getirir.
 * KPI bilgileri, son satışlar, kritik stoklar ve grafik verileri backend'den alınır.
 */
export async function getDashboard() {
  return httpGet("/api/dashboard");
}