# ChartPro

A powerful charting library for creating interactive and dynamic charts.

## Features

- Easy-to-use API
- Multiple chart types support
- Customizable themes and styling
- Responsive design
- Interactive animations

## Installation

```bash
npm install chartpro
```

## Usage

```javascript
import ChartPro from 'chartpro';

const chart = new ChartPro({
  element: '#chart-container',
  type: 'line',
  data: {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
    datasets: [{
      label: 'Sales',
      data: [12, 19, 3, 5, 2]
    }]
  }
});
```

## Chart Types

- Line Charts
- Bar Charts
- Pie Charts
- Area Charts
- Scatter Plots

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License
