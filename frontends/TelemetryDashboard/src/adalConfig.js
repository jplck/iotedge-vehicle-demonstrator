import { AuthenticationContext, adalFetch, withAdalLogin, adalGetToken } from 'react-adal';

//const endpoint = "https://graph.microsoft.com";

export const adalConfig = {
  tenant: process.env.REACT_APP_TENANT_ID,
  clientId: process.env.REACT_APP_CLIENT_ID,
  endpoints: {
    vehicleServiceApi: process.env.REACT_APP_SERVICES_CLIENT_ID,
    signalRServiceApi: process.env.REACT_APP_SIGNALR_CLIENT_ID
  },
  cacheLocation: 'localStorage',
};

class AdalContext {
  authContext;
  constructor() {
    this.authContext = new AuthenticationContext(adalConfig);
  }
  get AuthContext() {
    return this.authContext;
  }
  
  GetToken(tokenEndpoint) {
    return adalGetToken(this.authContext, tokenEndpoint);
  }
  LogOut() {
    this.authContext.logOut();
  }
}
const adalContext = new AdalContext();
export default adalContext;

export const authContext = new AuthenticationContext(adalConfig);
 
export const adalVehicleServicesFetch = (fetch, url, options) =>
  adalFetch(authContext, adalConfig.endpoints.vehicleServiceApi, fetch, process.env.REACT_APP_SERVICES_URL + url, options);
 
export const adalSignalRServiceFetch = (fetch, url, options) =>
  adalFetch(authContext, adalConfig.endpoints.signalRServiceApi, fetch, process.env.REACT_APP_HUB_URL + url, options);

export const withAdalLoginApi = withAdalLogin(authContext, adalConfig.endpoints.vehicleServiceApi);