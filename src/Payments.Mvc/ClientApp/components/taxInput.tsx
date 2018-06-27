import * as React from 'react';

interface IProps {
    value: number;
    onChange: (value: string) => void;
}

interface IState {
    hasTax: boolean;
    value: string;
}

export default class DiscountInput extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);

        this.state = {
            hasTax: !!props.value,
            value: props.value,
        };
    }

    public componentWillReceiveProps(nextProps) {
        this.setState({
            value: nextProps.value,
        });
    }

    public render() {
        const { onChange } = this.props;
        const { value } = this.state;

        if (!this.state.hasTax) {
            return (
                <button className="btn-plain primary-color" onClick={this.addTax}>
                    <i className="fa fa-plus" /> Add tax
                </button>
            );
        }

        return (
            <div className="input-group">
                <input
                    type="number"
                    min="0"
                    step="0.0001"
                    className="form-control"
                    placeholder=""
                    value={value}
                    onBlur={(e) => { onChange(e.target.value) }}
                    onChange={(e) => { this.setState({ value: e.target.value }); }}
                />
                <div className="input-group-append">
                    <span className="input-group-text">%</span>
                </div>
            </div>
        );
    }

    private addTax = () => {
        this.setState({ hasTax: true });
    }
}
