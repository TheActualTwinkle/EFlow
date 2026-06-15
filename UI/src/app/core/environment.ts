declare global {
  interface Window {
    __EFLOW_CONFIG__?: {
      apiBaseUrl?: string;
      bookingApiBaseUrl?: string;
      dataImportApiBaseUrl?: string;
    };
  }
}

export const bookingApiBaseUrl = window.__EFLOW_CONFIG__?.bookingApiBaseUrl ?? window.__EFLOW_CONFIG__?.apiBaseUrl ?? '/api/booking';
export const dataImportApiBaseUrl = window.__EFLOW_CONFIG__?.dataImportApiBaseUrl ?? '/api/data-import';
export const apiBaseUrl = bookingApiBaseUrl;
