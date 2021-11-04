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
        """ Returns tree iterator (can be used only in for loop) """
        ...

    @property
    @abc.abstractmethod
    def buildings(self) -> iterobj.GameObjectIterator[objects.Building, in_.BuildingData]:
        """ Returns building iterator (can be used only in for loop) """
        ...

    @property
    @abc.abstractmethod
    def nodes(self) -> iterobj.GameObjectIterator[objects.Node, in_.NetNodeData]:
        """ Returns node iterator (can be used only in for loop) """
        ...

    @property
    @abc.abstractmethod
    def props(self) -> iterobj.GameObjectIterator[objects.Prop, in_.PropData]:
        """ Returns prop iterator (can be used only in for loop) """
        ...

    @property
    @abc.abstractmethod
    def segments(self) -> iterobj.GameObjectIterator[objects.Segment, in_.NetSegmentData]:
        """ Returns segment iterator (can be used only in for loop) """
        ...

    @abc.abstractmethod
    def get_prop(self, id_: int) -> objects.Prop:
        """ Returns prop object from its id """
        ...

    @abc.abstractmethod
    def get_tree(self, id_: int) -> objects.Tree:
        """ Returns tree object from its id """
        ...

    @abc.abstractmethod
    def get_building(self, id_: int) -> objects.Building:
        """ Returns building object from its id """
        ...

    @abc.abstractmethod
    def get_node(self, id_: int) -> objects.Node:
        """ Returns node object from its id """
        ...

    @abc.abstractmethod
    def get_segment(self, id_: int) -> objects.Segment:
        """ Returns segment object from its id """
        ...

    @abc.abstractmethod
    def create_prop(self,
        position: utils.IPositionable, prefab_name: str, angle: float
    ):
        """ Creates prop """
        ...

    @abc.abstractmethod
    def create_tree(self, position: utils.IPositionable, prefab_name: str):
        """ Creates tree """
        ...

    @abc.abstractmethod
    def create_building(self,
        position: utils.IPositionable, type_: str, angle: float = 0
    ) -> Optional[objects.Building]:
        """ Creates building """
        ...

    @abc.abstractmethod
    def create_node(self,
        position: utils.IPositionable, prefab: str | objects.NetPrefab
    ) -> Optional[objects.Node]:
        """ Creates node (eg. segment junction) """
        ...

    @abc.abstractmethod
    def create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[objects.Segment]:
        """ 
        Creates segment (road).
        Don't use this method, but create_segments(..)
        """
        ...

    @abc.abstractmethod
    def create_segments(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[List[objects.Segment]]:
        """ Creates segments (road). """
        ...

    @abc.abstractmethod
    def begin_path(
        self, start_node: utils.IPositionable, options: Any
    ) -> objects.PathBuilder:
        """
        Starts a road on a given point.
        Call path_to(..) on the returned object to build a road
        """
        ...

    @abc.abstractmethod
    def draw_vector(self,
        vector: utils.IPositionable, origin: utils.IPositionable,
        color: str = 'red', length: float = 20, size: float = 0.1
    ) -> objects.RenderableObjectHandle:
        """
        Draws line on map.
        Returns handle which can be used to delete the line.
        Use clear() to delete all lines.
        """
        ...
    
    @abc.abstractmethod
    def draw_circle(self,
        position: utils.IPositionable, radius: float = 5, color: str = 'red'
    )  -> objects.RenderableObjectHandle:
        """
        Draws circle on map.
        Returns handle which can be used to delete the circle.
        Use clear() to delete all lines
        """
        ...

    @abc.abstractmethod
    def remove_render_object(self, id_: int):
        ...

    @abc.abstractmethod
    def clear(self):
        """ Clears all lines drawn on map """
        ...

    @abc.abstractmethod
    def get_net_prefab(self, name: str) -> objects.NetPrefab:
        """
        Returns network prefab (used to build nodes and segments).
        Eg. 'Basic road'
        """
        ...

    @abc.abstractmethod
    def is_prefab(self, name: str) -> bool:
        """ Returns if name is a valid prefab (network, building, tree etc.) """
        ...

    @abc.abstractmethod
    def terrain_height(self, pos: utils.IPositionable) -> float:
        """ Returns terrain height at a given point (height is Y coord) """
        ...

    @abc.abstractmethod
    def surface_level(self, pos: utils.IPositionable) -> float:
        """ Returns terrain height inlucing water level at a given point """
        ...
