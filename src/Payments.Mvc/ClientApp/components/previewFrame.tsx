import * as React from 'react';

import { Invoice } from '../models/Invoice';
import { Team } from '../models/Team';

interface IProps {
    invoice: Partial<Invoice>;
    team: Team;
}

export default class PreviewFrame extends React.PureComponent<IProps, {}> {

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

    public render() {
        const { invoice, team } = this.props;
        const { customer, memo, discount, dueDate, taxPercent, items } = invoice;

        const action = '/Payments/PreviewFromJson';

        const value = JSON.stringify({
            customerEmail: customer ? customer.email : '',
            discount,
            dueDate,
            items,
            memo,
            taxPercent,
            teamName: team.name,
        });

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