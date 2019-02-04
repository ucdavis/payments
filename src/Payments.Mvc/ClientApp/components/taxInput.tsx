import * as React from 'react';
 
interface IProps {
    value: number;
    onChange: (value: string) => void;
}

interface IState {
    hasTax: boolean;
    value: string;
}

export default class TaxInput extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);

        this.state = {
            hasTax: !!props.value,
            value: props.value.toFixed(4),
        };
    }

    public componentWillReceiveProps(nextProps: IProps) {
        this.setState({
            value: nextProps.value.toFixed(4),
        });
    }

    public render() {
        const { onChange } = this.props;
        const { value } = this.state;

        if (!this.state.hasTax) {
            return (
                <button className="btn-plain primary-color" onClick={this.addTax}>
                    <i className="fas fa-plus mr-2" /> Add tax
                </button>
            );
        }

        return (
            <div className="input-group">
                <input
                    type="number"
                    min="0.001"
                    step="0.0001"
                    className="form-control"
                    placeholder="0.00"
                    value={value}
                    onBlur={(e) => { onChange(e.target.value) }}
                    onChange={(e) => { this.setState({ value: e.target.value }); }}
                    required={true}
                />
                <div className="input-group-append">
                    <span className="input-group-text">%</span>
                </div>
                <div className="input-group-append">
                    <span className="input-group-text">
                        <a href=""><i className="fas fa-search" /></a>
                    </span>
                </div>
                <div className="invalid-feedback">
                    Set a tax or remove.
                </div>
            </div>
        );
    }

    private addTax = () => {
        this.setState({ hasTax: true });
    }
}
