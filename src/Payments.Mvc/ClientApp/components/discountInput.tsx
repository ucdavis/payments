import * as React from 'react';
 
interface IProps {
    value: number;
    onChange: (value: string) => void;
}

interface IState {
    hasDiscount: boolean;
}

export default class DiscountInput extends React.Component<IProps, IState> {

    public render() {
        const { value, onChange } = this.props;

        if (!this.state.hasDiscount) {
            return (
                <button className="btn btn-link" onClick={this.addDiscount}>
                    <i className="fa fa-plus" /> Add coupon
                </button>
            );
        }
        
        return (
            <input
                type="number"
                min="0"
                step="0.01"
                className="form-control"
                placeholder="0.00"
                value={value}
                onChange={(e) => { onChange(e.target.value) }}
            />
        );
    }

    private addDiscount = () => {
        this.setState({ hasDiscount: true });
    }
}