import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader';

import CreateInvoiceContainer from '../containers/CreateInvoiceContainer';

import { Team } from '../models/Team';

declare var team: Team;

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render( 
        <AppContainer>
            <CreateInvoiceContainer team={team} />
        </AppContainer>, 
        document.getElementById('react-app') 
    ); 
} 
 
renderApp(); 
 
// Allow Hot Module Replacement 
if (module.hot) { 
    module.hot.accept('../containers/CreateInvoiceContainer', () => { 
        renderApp(); 
    }); 
} 