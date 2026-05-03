declare global {
  interface Window {
    __EFLOW_CONFIG__?: {
      apiBaseUrl?: string;
    };
  }
}

export const apiBaseUrl = window.__EFLOW_CONFIG__?.apiBaseUrl ?? '/api';
