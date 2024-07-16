import torch
import matplotlib.pyplot as plt
import cv2
from ultralytics import YOLO
from collections import defaultdict
import os
import json

def ExternalCall(imageName):
    image_path = 'D:\\temp\\'+imageName
    image_path_save = 'D:\\temp\\'+'result_'+imageName
    model = YOLO(r'C:\Users\PCZONE.GE\source\repos\EKF_AI\ObjectDetectionPy\best_weights.pt')

    # Загрузка изображения
    img = cv2.imread(image_path)
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    
    # Выполнение предсказания
    results = model(img_rgb, verbose=False)
    
    # Получение bounding boxes, меток и уверенности
    boxes = results[0].boxes.data.cpu().numpy()  # Извлечение данных боксов
    labels = model.names
    
    # Словарь для подсчета количества объектов каждого класса
    class_counts = defaultdict(int)
    
    # Словарь для результата
    result_dict = {'boxes': [], 'labels': [], 'confidences': []}
    
    # Отображение результатов
    fig, ax = plt.subplots(1, figsize=(12, 9))
    ax.imshow(img_rgb)
    
    for box in boxes:
        x_min, y_min, x_max, y_max, conf, class_id = box[:6]
        label = labels[int(class_id)]
        
        # Увеличиваем счетчик для текущего класса
        class_counts[label] += 1
        
        # Добавление данных в результат
        result_dict['boxes'].append([x_min, y_min, x_max, y_max])
        result_dict['labels'].append(label)
        result_dict['confidences'].append(conf)
        
        # Создание bounding box
        rect = plt.Rectangle((x_min, y_min), x_max - x_min, y_max - y_min, fill=False, edgecolor='red', linewidth=2)
        ax.add_patch(rect)
        
        # Добавление метки
        ax.text(x_min, y_min - 10, f"{label} ({conf:.2f})", bbox=dict(facecolor='yellow', alpha=0.5), fontsize=12, color='black')
    
    plt.axis('off')
    plt.savefig(image_path_save, bbox_inches='tight', pad_inches=0)
    plt.close(fig)
    
    # Вывод количества объектов каждого класса
    result_dict['class_counts'] = dict(class_counts)
    
    return json.dumps(class_counts, indent=4)

# print(ExternalCall('c72482ae-506c-4694-9911-e6cad6a868e6.jpg'))



