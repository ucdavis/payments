import * as React from 'react';

interface IProps {
  className?: string;

  value: number;
  onChange: (value: number) => void;

  min?: number;
  max?: number;
  step?: number;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;

  format?: (value: number) => string;
  inputRef?: ((el: HTMLInputElement | null) => void) | React.RefObject<HTMLInputElement>;
}

interface IState {
  type: string;
  value: string;

  // Fix for: https://bugzilla.mozilla.org/show_bug.cgi?id=1057858
  noopBlur: boolean;
}

export default class NumberControl extends React.PureComponent<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.state = {
      type: 'text',
      value: this.valueToString(props.value),

      noopBlur: false
    };
  }

  public componentDidUpdate(prevProps: IProps) {
    if (prevProps.value !== this.props.value) {
      this.setState({
        value: this.valueToString(this.props.value)
      });
    }
  }

  public render() {
    const {
      className,
      min,
      max,
      step,
      placeholder,
      required,
      inputRef,
      disabled
    } = this.props;
    const { type, value } = this.state;

    return (
      <input
        type={type}
        min={min}
        max={max}
        step={step}
        className={`form-control ${className}`}
        placeholder={placeholder}
        value={value}
        onFocus={this.onFocus}
        onBlur={this.onBlur}
        onChange={e => {
          this.setState({ value: e.target.value });
        }}
        required={required}
        ref={inputRef}
        disabled={disabled}
      />
    );
  }

  private valueToString = (value: number) => {
    if (this.props.format) {
      return this.props.format(value);
    }

    if (value === 0) {
      return '';
    }

    return value.toFixed(2);
  };

  private onFocus = (event: React.FocusEvent<HTMLInputElement>) => {
    this.setState({ noopBlur: true });
    this.setState({
      noopBlur: true,
      type: 'number'
    });

    setTimeout(() => this.setState({ noopBlur: false }), 100);
  };

  private onBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    if (this.state.noopBlur) {
      return;
    }

    let discount = Number(event.target.value);
    if (isNaN(discount)) {
      discount = 0;
    }

    this.props.onChange(discount);
    this.setState({ type: 'text' });
  };
}
