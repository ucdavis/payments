import * as React from 'react';

import { PreviewInvoice } from '../models/PreviewInvoice';

interface IProps {
    invoice: PreviewInvoice;
}

export default class PreviewFrame extends React.Component<IProps, {}> {

    private _previewForm: HTMLFormElement;

    public componentDidMount() {
        this.loadIFrame();
    }

    public componentDidUpdate() {
        this.loadIFrame();
    }

    public componentWillUnmount() {
        this._previewForm = undefined;
    }

    public shouldComponentUpdate(nextProps: IProps) {
        // perform cheap compare via serialization
        const model = JSON.stringify(this.props.invoice);
        const newModel = JSON.stringify(nextProps.invoice);

        return model.localeCompare(newModel) !== 0;
    }

    public render() {
        const { invoice } = this.props;

        const action = '/Payments/PreviewFromJson';

        const value = JSON.stringify(invoice);

        return (
            <div>
                <form method="post" action={action} target="iframe_preview" ref={r => this._previewForm = r}>
                    <input type="hidden" name="json" value={value} />
                </form>
                <div className="d-flex" style={{ minHeight: '60vh' }}>
                    <iframe name="iframe_preview" style={{ border: '0px none', minHeight: '100%', position: 'relative', width: '100%' }} />
                </div>
            </div>
        );
    }

    private loadIFrame = () => {
        if (this._previewForm) {
            this._previewForm.submit();
        }
    }
}
