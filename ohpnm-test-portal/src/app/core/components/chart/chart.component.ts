import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import {
  ArcElement,
  CategoryScale,
  Chart,
  ChartData,
  ChartOptions,
  ChartType,
  DoughnutController,
  Filler,
  Legend,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Tooltip,
} from 'chart.js';

Chart.register(
  DoughnutController,
  ArcElement,
  LineController,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  Filler,
  Tooltip,
  Legend
);

@Component({
  selector: 'app-chart',
  standalone: true,
  template: `<div class="chart-box"><canvas #canvas [attr.aria-label]="ariaLabel" role="img"></canvas></div>`,
  styles: [':host { display: block; position: relative; } .chart-box { position: relative; width: 100%; height: 100%; }'],
})
export class ChartComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) type!: ChartType;
  @Input({ required: true }) data!: ChartData<any>;
  @Input() options: ChartOptions<any> = {};
  @Input() ariaLabel = 'Chart';

  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;
  private renderedType?: ChartType;

  ngAfterViewInit(): void {
    this.render();
  }

  ngOnChanges(): void {
    if (!this.canvas) return;
    if (this.chart && this.renderedType === this.type) {
      this.chart.data = this.data;
      this.chart.options = { responsive: true, maintainAspectRatio: false, ...this.options };
      this.chart.update();
    } else {
      this.render();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private render(): void {
    this.chart?.destroy();
    this.renderedType = this.type;
    this.chart = new Chart(this.canvas.nativeElement, {
      type: this.type,
      data: this.data,
      options: { responsive: true, maintainAspectRatio: false, ...this.options },
    });
  }
}
