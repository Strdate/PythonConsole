from __future__ import annotations

import abc
from typing import Any, List, Optional
from . import iterobj
from .. import objects, in_, utils
from ... import meta

class BaseGame(metaclass=meta.Singleton):
    @property
    @abc.abstractmethod
    def trees(self) -> iterobj.GameObjectIterator[objects.Tree, in_.TreeData]:
        ...

    @property
    @abc.abstractmethod
    def buildings(self) -> iterobj.GameObjectIterator[objects.Building, in_.BuildingData]:
        ...

    @property
    @abc.abstractmethod
    def nodes(self) -> iterobj.GameObjectIterator[objects.Node, in_.NetNodeData]:
        ...

    @property
    @abc.abstractmethod
    def props(self) -> iterobj.GameObjectIterator[objects.Prop, in_.PropData]:
        ...

    @property
    @abc.abstractmethod
    def segments(self) -> iterobj.GameObjectIterator[objects.Segment, in_.NetSegmentData]:
        ...

    @abc.abstractmethod
    def get_prop(self, id_: int) -> objects.Prop:
        ...

    @abc.abstractmethod
    def get_tree(self, id_: int) -> objects.Tree:
        ...

    @abc.abstractmethod
    def get_building(self, id_: int) -> objects.Building:
        ...

    @abc.abstractmethod
    def get_node(self, id_: int) -> objects.Node:
        ...

    @abc.abstractmethod
    def get_segment(self, id_: int) -> objects.Segment:
        ...

    @abc.abstractmethod
    def create_prop(self,
        position: utils.IPositionable, prefab_name: str, angle: float
    ):
        ...

    @abc.abstractmethod
    def create_tree(self, position: utils.IPositionable, prefab_name: str):
        ...

    @abc.abstractmethod
    def create_building(self,
        position: utils.IPositionable, type_: str, angle: float = 0
    ) -> Optional[objects.Building]:
        ...

    @abc.abstractmethod
    def create_node(self,
        position: utils.IPositionable, prefab: str | objects.NetPrefab
    ) -> Optional[objects.Node]:
        ...

    @abc.abstractmethod
    def create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[objects.Segment]:
        ...

    @abc.abstractmethod
    def create_segments(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[List[objects.Segment]]:
        ...

    @abc.abstractmethod
    def begin_path(
        self, start_node: utils.IPositionable, options: Any
    ) -> objects.PathBuilder:
        ...

    @abc.abstractmethod
    def draw_vector(self,
        vector: utils.IPositionable, origin: utils.IPositionable,
        color: str = 'red', length: float = 20, size: float = 0.1
    ) -> objects.RenderableObjectHandle:
        ...
    
    @abc.abstractmethod
    def draw_circle(self,
        position: utils.IPositionable, radius: float = 5, color: str = 'red'
    )  -> objects.RenderableObjectHandle:
        ...

    @abc.abstractmethod
    def remove_render_object(self, id_: int):
        ...

    @abc.abstractmethod
    def clear(self):
        ...

    @abc.abstractmethod
    def get_net_prefab(self, name: str) -> objects.NetPrefab:
        ...

    @abc.abstractmethod
    def is_prefab(self, name: str) -> bool:
        ...

    @abc.abstractmethod
    def terrain_height(self, pos: utils.IPositionable) -> float:
        ...

    @abc.abstractmethod
    def surface_level(self, pos: utils.IPositionable) -> float:
        ...