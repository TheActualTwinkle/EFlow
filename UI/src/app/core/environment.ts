declare global {
  interface Window {
    __EFLOW_CONFIG__?: {
      bookingApiBaseUrl?: string;
      dataImportApiBaseUrl?: string;
    };
  }
}

export const bookingApiBaseUrl = window.__EFLOW_CONFIG__?.bookingApiBaseUrl ?? '/api/booking';
export const dataImportApiBaseUrl = window.__EFLOW_CONFIG__?.dataImportApiBaseUrl ?? '/api/data-import';
