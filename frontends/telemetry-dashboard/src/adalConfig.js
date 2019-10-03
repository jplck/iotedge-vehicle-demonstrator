import { AuthenticationContext, adalFetch, withAdalLogin, adalGetToken } from 'react-adal';

const endpoint = "https://graph.microsoft.com";

export const adalConfig = {
  tenant: process.env.REACT_APP_TENANT_ID,
  clientId: process.env.REACT_APP_CLIENT_ID,
  endpoints: {
    api: endpoint,
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
  
  GetToken() {
    return adalGetToken(this.authContext, endpoint);
  }
  LogOut() {
    this.authContext.logOut();
  }
}
const adalContext = new AdalContext();
export default adalContext;

export const authContext = new AuthenticationContext(adalConfig);
 
export const adalApiFetch = (fetch, url, options) =>
  adalFetch(authContext, adalConfig.endpoints.api, fetch, url, options);
 
export const withAdalLoginApi = withAdalLogin(authContext, adalConfig.endpoints.api);