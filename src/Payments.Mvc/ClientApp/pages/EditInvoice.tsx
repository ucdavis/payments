import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader'; 
import EditInvoiceContainer from '../containers/EditInvoiceContainer';
import { Invoice } from '../models/Invoice';

declare var model: Invoice;
console.log("model:", model);

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render( 
        <AppContainer>
            <EditInvoiceContainer invoice={model} />
        </AppContainer>, 
        document.getElementById('react-app') 
    ); 
} 
 
renderApp(); 
 
// Allow Hot Module Replacement 
if (module.hot) { 
    module.hot.accept('../containers/EditInvoiceContainer', () => { 
        renderApp(); 
    }); 
} 