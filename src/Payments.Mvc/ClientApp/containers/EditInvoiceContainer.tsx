import * as React from 'react';

interface IProps {
    invoice: any;
}

interface IState {
    title: string;
}

export default class EditInvoiceContainer extends React.Component<IProps, IState> {
    public render() {
        const { invoice } = this.props;
        
        return (
            <div>
                Edit Invoice
            </div>
        );
    }
}
