import * as React from 'react';
 
interface IProps {
    value: number;
    onChange: (value: string) => void;
}

interface IState {
    hasDiscount: boolean;
}

export default class DiscountInput extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);

        this.state = {
            hasDiscount: !!props.value,
        };
    }

    public render() {
        const { value, onChange } = this.props;

        if (!this.state.hasDiscount) {
            return (
                <button className="btn btn-link" onClick={this.addDiscount}>
                    <i className="fas fa-plus mr-2" /> Add coupon
                </button>
            );
        }
        
        return (
            <div className="input-group">
                <div className="input-group-prepend">
                    <span className="input-group-text">$</span>
                </div>
                <input
                    type="number"
                    min="0.01"
                    step="0.01"
                    className="form-control"
                    placeholder="0.00"
                    value={value}
                    onChange={(e) => { onChange(e.target.value) }}
                    required={true}
                />
                <div className="invalid-feedback">
                    Set a discount or remove.
                </div>
            </div>
        );
    }

    private addDiscount = () => {
        this.setState({ hasDiscount: true });
    }
}