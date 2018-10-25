import * as React from 'react';
import CurrencyControl from './currencyControl';
 
interface IProps {
    value: number;
    onChange: (value: number) => void;
}

interface IState {
    hasDiscount: boolean;
}

export default class DiscountInput extends React.PureComponent<IProps, IState> {

    private _inputRef: React.RefObject<HTMLInputElement>;

    constructor(props: IProps) {
        super(props);

        this.state = {
            hasDiscount: !!props.value,
        };

        this._inputRef = React.createRef<HTMLInputElement>();
    }

    public render() {
        const { onChange, value } = this.props;

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
                <CurrencyControl value={value} onChange={onChange} inputRef={this._inputRef} />
                <div className="invalid-feedback">
                    Set a discount or remove.
                </div>
            </div>
        );
    }

    private addDiscount = () => {
        this.setState({ hasDiscount: true });

        if (this._inputRef.current) {
            this._inputRef.current.focus();
        }
    }
}