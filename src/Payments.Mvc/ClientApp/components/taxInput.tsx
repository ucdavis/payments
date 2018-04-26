import * as React from 'react';
 
interface IProps {
    value: number;
    onChange: (value: string) => void;
}

interface IState {
    hasTax: boolean;
}

export default class DiscountInput extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);

        this.state = {
            hasTax: !!props.value,
        };
    }

    public render() {
        const { value, onChange } = this.props;

        if (!this.state.hasTax) {
            return (
                <button className="btn btn-link" onClick={this.addTax}>
                    <i className="fa fa-plus" /> Add tax
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

    private addTax = () => {
        this.setState({ hasTax: true });
    }
}