import pandas as pd
import matplotlib.pyplot as plt

# Чтение данных из CSV
data = pd.read_csv("queues.csv")

# Построение графика
plt.plot(data["Step"], data["Queue1"], label="СМО 1", color="red")
plt.plot(data["Step"], data["Queue2"], label="СМО 2", color="blue")
plt.xlabel("Шаг симуляции")
plt.ylabel("Длина очереди")
plt.title("Длина очередей по времени")
plt.legend()
plt.grid(True)
plt.tight_layout()

# Сохранение графика
plt.savefig("queues_plot.png")
plt.show()
