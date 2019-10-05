import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import * as serviceWorker from './serviceWorker';
import 'react-notifications-component/dist/theme.css'
import 'bootstrap/dist/css/bootstrap.min.css';
import { runWithAdal } from 'react-adal';
import adalContext, { authContext } from './adalConfig';
import hash from 'object-hash'

const DO_NOT_LOGIN = false;

runWithAdal(authContext, () => {
    adalContext.GetToken()
    .then(token => {
        var hashedUser = hash(authContext.getCachedUser().userName)
        ReactDOM.render(<App authToken={token} userId={hashedUser} />, document.getElementById('root'));

        serviceWorker.unregister();
   });
}, DO_NOT_LOGIN);