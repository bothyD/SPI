import matplotlib.pyplot as plt
import os

# Чтение данных из файла
with open('src/queue_lengths.txt', 'r') as f:
    data = f.readlines()

# Преобразование строк в списки
regular_lengths = list(map(int, data[0].strip().split(',')))
poisson_lengths = list(map(int, data[1].strip().split(',')))
erlang_lengths = list(map(int, data[2].strip().split(',')))

# Время (количество итераций)
time = range(len(regular_lengths))

# Построение графиков
plt.plot(time, regular_lengths, label='Регулярный поток')
plt.plot(time, poisson_lengths, label='Пуассоновский поток')
plt.plot(time, erlang_lengths, label='Поток Эрланга')

plt.xlabel('Время (итерации)')
plt.ylabel('Длина очереди')
plt.title('Длина очереди в зависимости от времени')
plt.legend()

# Создание папки src, если она не существует
if not os.path.exists('src'):
    os.makedirs('src')

# Сохранение графика в формате PNG в папку src
plt.savefig('src/queue_lengths.png')

# Отображение графика (опционально)
plt.show()
