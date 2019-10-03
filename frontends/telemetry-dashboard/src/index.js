import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import * as serviceWorker from './serviceWorker';
import { runWithAdal } from 'react-adal';
import adalContext, { authContext } from './adalConfig';
import 'react-notifications-component/dist/theme.css'

const DO_NOT_LOGIN = false;

runWithAdal(authContext, () => {
    adalContext.GetToken()
    .then(token => {
        console.log(token);
        ReactDOM.render(<App authToken={token}/>, document.getElementById('root'));

        serviceWorker.unregister();
   });
}, DO_NOT_LOGIN);