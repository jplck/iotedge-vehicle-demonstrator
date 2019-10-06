import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import * as serviceWorker from './serviceWorker';
import 'react-notifications-component/dist/theme.css'
import 'bootstrap/dist/css/bootstrap.min.css';
import { runWithAdal } from 'react-adal';
import adalContext, { authContext, adalConfig } from './adalConfig';
import hash from 'object-hash'

const DO_NOT_LOGIN = false;

runWithAdal(authContext, () => {
    adalContext.GetToken(adalConfig.endpoints.signalRServiceApi)
    .then(signalRToken => {
        var hashedUser = hash(authContext.getCachedUser().userName)

        ReactDOM.render(<App userId={hashedUser} signalRToken={signalRToken}/>, document.getElementById('root'));
        serviceWorker.unregister();

   });
}, DO_NOT_LOGIN);