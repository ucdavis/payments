import '../css/site.css';
import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader'; 
import CreateInvoiceContainer from '../containers/CreateInvoiceContainer';
import { Invoice } from '../models/Invoice';

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render( 
        <AppContainer>
            <CreateInvoiceContainer />
        </AppContainer>, 
        document.getElementById('react-app') 
    ); 
} 
 
renderApp(); 
 
// Allow Hot Module Replacement 
if (module.hot) { 
    module.hot.accept('./', () => { 
        renderApp(); 
    }); 
} 