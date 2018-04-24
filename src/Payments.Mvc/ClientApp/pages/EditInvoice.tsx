import '../css/site.css';
import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader'; 
import EditInvoiceContainer from '../containers/EditInvoiceContainer';
import { Invoice } from '../models/Invoice';

const invoice: Invoice = {
    items: [{
        id: 1,
        description: '',
        price: 0,
        quantity: 0,
    }],
};

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render( 
        <AppContainer>
            <EditInvoiceContainer invoice={invoice} />
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